# Ruby

## General Information

Code was written in Ruby 2.3.1 (Last tested on Ruby 3.1.2)

Two different executables exist depending on what you are trying to do:
- retrieve_jobs.rb - basic call showing how to collect job data over the Jobs API
- work_time_estimate - calculates a work time estimate of the presses each 5 minutes

## How To Run / Program Information

Before you can run the code, you need to provide the Key/Secret in retrieve_jobs.rb.  

You may also need to configure a web proxy in the jobs_api.rb file by uncommenting and modifying the proxy definition

Run on the command line using ```ruby work_time_estimate.rb```

