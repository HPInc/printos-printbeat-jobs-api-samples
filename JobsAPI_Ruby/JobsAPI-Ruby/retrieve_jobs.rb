#
#  In this sample we are showing the basics of establishing a local view of queued press jobs that exist in an account and updating that view every few minutes.
#  In this example the work time estimate for all the jobs on a press including whats left of the currently printed job will printed to the console at the interval set in 'work_time_estimate.rb' (5 minutes in the example).
#  In this example we keep the current view of queued and printing jobs in an in-memory list. You may want to persist your view in a formal database.
#
module Jobs
  require 'json'
  require 'ostruct'
  require 'zlib'
  require 'stringio'
  require './job'
  require './jobs_api'
  require './job_context'
  require './ink'
  require './substrate'
  class RetrieveJobs
    #Access Credentials

    def initialize
      @jobComparator = JobComparator.new
      @queuedJobs = Hash.new
      @printingJobs = Hash.new
      @baseUrl = "https://printos.api.hp.com/jobs-service";        #use for production account

      #Access Credentials
      ###########################################################################################
      #
      @@key = nil # insert your account PrintOS Jobs key here
      @@secret = nil # insert your account PrintOS Jobs secret here
      #
      ###########################################################################################

      #Set of properties to ask for, if there are any additional properties needed make sure to add them to job.rb
      @propertiesList = 'deviceId,jobId,jobName,jobProgress,jobSubmitTime,jobCompleteTime,jobType,parentJobId,jobLastEventTime,jobWorkTimeEstimate,location,marker'
      @startMarker = 0
      @jobsApi = JobsApi.new(@baseUrl, @@key, @@secret)
    end

    def getJobSet(response)
      #Check status of response. In cases where the frequency of the call rate is too high or the amount of calls exceeds
      #the set limit, a 429 Too Many Requests error will be returned.
      if (response.code != '200') then
        return Array.new
      end

      jobSet = JSON.parse(response.body, object_class: OpenStruct)
      return jobSet
    end

    def humanReadableTime(secs)
      return [[60, :seconds], [60, :minutes], [24, :hours], [1000, :days]].map {|count, name|
        if secs > 0
          secs, n = secs.divmod(count)
          "#{n.to_i} #{((n.to_i < 2) ? name[0..-2] : name)}"
        end
      }.compact.reverse.join(' ')
    end

    ################################################################################
    def buildUpPressQueues
      startMarker = 0
      limit = 10000
      # get most recent 1000 press jobs.
      response = @jobsApi.retrieveJobs(JobContext::Press, limit, @propertiesList, nil, startMarker, "forward")
      jobSet = self.getJobSet(response)
      if (jobSet == nil)
        return 0
      end
      continueSync = (jobSet.length == 1000)
      if jobSet.count > 0 then
        startMarker = jobSet.last.marker
      end
      while (continueSync)
        if jobSet.count > 0 then
          startMarker = jobSet.last.marker
        end
        if jobSet.count < limit then
          continueSync = false
        else
          response = @jobsApi.retrieveJobs(JobContext::Press, limit, @propertiesList, null, startMarker, "forward")
          jobSet = self.getJobSet(response)
        end
      end

      # filter down the jobs to consider only queued jobs.
      queuedJobs = jobSet.select {|job| job[:location] == 'QUEUED' && job[:jobProgress] == 'QUEUED'}
      if (queuedJobs != nil && queuedJobs.count > 0)
        @queuedJobs = queuedJobs.map {|job| [job.jobId, job]}.to_h
      end


      # check if there are jobs which are currently printing.
      printingJobs = jobSet.select {|job| job[:jobProgress] == 'PRINTING'}
      if (printingJobs != nil && printingJobs.count > 0)
        @printingJobs = printingJobs.map {|job| [job.jobId, job]}.to_h
      end
      return startMarker
    end


    def getLatestWorkEstimates(startMarker)

      limit = 1000
      # get most recent press jobs.
      response = @jobsApi.retrieveJobs(JobContext::Press, limit, @propertiesList, nil, startMarker, "forward")
      jobSet = self.getJobSet(response)
      if (jobSet != nil && jobSet.count > 0)
        startMarker = jobSet.last.marker
        # filter down the jobs to consider only queued jobs.
        queuedJobs = jobSet.select {|job| job[:location] == 'QUEUED' && job[:jobProgress] == 'QUEUED'}
        if (queuedJobs != nil && queuedJobs.count > 0)
          updatedQueued = queuedJobs.map {|job| [job.jobId, job]}.to_h
          @queuedJobs.merge!(updatedQueued) # Update any new jobs loaded to the print queue.
        end
        # check if there are jobs which are currently printing.
        printingJobs = jobSet.select {|job| job[:jobProgress] == 'PRINTING'}
        if (printingJobs != nil && printingJobs.count > 0)
          @printingJobs.merge!(printingJobs.map {|job| [job.jobId, job]}.to_h) # update currently printing jobs.
        end

        # validate queued jobs map, remove any jobs which are not queued anymore.
        @queuedJobs.each do |jobId, job|
          isStillQueuedJob = jobSet.select {|job| job[:jobId] == jobId}
          if (isStillQueuedJob != nil && (isStillQueuedJob[0].location != 'QUEUED' || isStillQueuedJob[0].jobProgress != 'QUEUED'))
            @queuedJobs.delete(jobId)
          end
        end

        # validate printing jobs map, remove any jobs which have finished printing.
        @printingJobs.each do |jobId, job|
          isStillPrintingJob = jobSet.select {|job| job[:jobId] == jobId}
          if (isStillPrintingJob != nil && isStillPrintingJob[0].jobProgress != 'PRINTING')
            @printingJobs.delete(jobId)
          end
        end

      end

      @jobsPrintingOnDeviceMap = nil
      timestamp = Time.now
      # calculate how much time left for each printing job

      if (@printingJobs != nil && @printingJobs.count > 0)
        @jobsPrintingOnDeviceMap = Hash.new
        @printingJobs.each do |jobId, job|
          approxPrintStart = Time.parse(job.jobLastEventTime[0..-7]) # assuming the last event received was the change of jobProgress to printing as the start of printing time.
          @jobsPrintingOnDeviceMap[job.deviceId] = job.jobWorkTimeEstimate - (timestamp - approxPrintStart) # subtract the elapsed time from the estimated time.
        end
      end

      # calculate total work time estimate for each device in addition to time left for present printing job.
      queuedJobs = @queuedJobs.map {|jobId, job| job}
      devices = queuedJobs.uniq {|queuedJob| [queuedJob.deviceId]}
      deviceIds = devices.map {|job| job.deviceId}
      if (devices.count == 0)
        puts 'No pending print jobs.'
      else
        deviceIds.each do |deviceId|
          puts ('Work estimate on device:' + deviceId + ' = ' +
            humanReadableTime((queuedJobs.select {|job| job[:deviceId] == deviceId}.inject(0) {|sum, job| sum + job.jobWorkTimeEstimate}) + # sum all jobs work estimates on this specific device
                                (((@jobsPrintingOnDeviceMap != nil) && (@jobsPrintingOnDeviceMap.key? deviceId)) ? @jobsPrintingOnDeviceMap[deviceId] : 0)).to_s) # add the time left for the job currently printing on the device
        end
      end
      return startMarker
    end

  end
end
