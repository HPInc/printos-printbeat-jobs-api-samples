module Jobs
  class Job
    def initialize(deviceId, jobId, jobName, jobProgress, jobSubmitTime,jobLastEventTime, jobType,parentJobId,jobCompleteTime, jobWorkTimeEstimate, location, marker)
      @deviceId = deviceId
      @jobId = jobId
      @jobName = jobName
      @jobProgress = jobProgress
      @jobSubmitTime = jobSubmitTime
      @jobType = jobType
      @jobWorkTimeEstimate = jobWorkTimeEstimate
      @location = location
      @marker = marker
      @jobLastEventTime = jobLastEventTime
      @parentJobId = parentJobId
      @jobCompleteTime = jobCompleteTime
    end

    attr_accessor :deviceId
    :jobId
    :jobName
    :jobProgress
    :jobSubmitTime
    :jobType
    :jobWorkTimeEstimate
    :jobLastEventTime
    :parentJobId
    :location
    :marker
    :jobCompleteTime
  end
  class JobComparator
    def Equals (job1, job2)
      return job1.jobId == job2.jobId && job1.deviceId == job2.deviceId
    end
  end
end
