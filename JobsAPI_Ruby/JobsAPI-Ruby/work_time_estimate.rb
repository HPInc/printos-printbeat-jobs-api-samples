module Jobs
  require './retrieve_jobs'
class WorkTimeEstimate
  @retrieveJobs = RetrieveJobs.new
  # create a snapshot view of queued jobs in addition to currently printing jobs.
  startMarker = @retrieveJobs.buildUpPressQueues

  while true
    # Every five minutes, print out the work estimate for each press device including the remainder of the currently printing job.
    startMarker = @retrieveJobs.getLatestWorkEstimates(startMarker)
    sleep(5*60) # 5 minutes
  end
end
end
