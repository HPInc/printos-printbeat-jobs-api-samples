using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace Jobs
{
    /// In this sample we are showing the basics of establishing a local view of jobs in an account and updating that view every few minutes.
    /// The example shows 2 steps in the process. 1) The initial establishment of the view by large page queries of all records in the system. 2) The periodic 
    /// query and processing of modification since that last update.
    /// Once you have this view of your jobs, you can analyze the data as you see fit. 
    /// In this example the only "analysis" of the data is to count the various types of jobs, and get an overall total of worktime estimates of press jobs that are queued.
    /// In this example we keep the current view jobs in an in-memory list. You may want to persist your view in a formal database.
    class RetrieveJobs
    {
        //Access Credentials
        private static string baseUrl = "https://printos.api.hp.com/jobs-service";        //use for production account
        //private static string baseUrl = "https://stage.printos.api.hp.com/jobs-service";    //use for staging account

        private static string key = "";
        private static string secret = "";

        private static JobComparator comparator = new JobComparator();
        private static List<Job> jobs;
        private static JobsAPI jobsApi;
        private static Timer timer;

        //Set of properties to ask for, if there are any additional properties needed make sure to add them to Job.cs
        private static string propertiesList = "deviceId,jobId,jobName,jobProgress,jobSubmitTime,jobType,jobWorkTimeEstimate,location,marker"; 
        private static long startMarker = 0;

        static void Main(string[] args)
        {
            //Forcing TLS to 1.2 prevents an issue with older .NET frameworks defaulting to 1.0 which is not supported.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //Review the JobsAPI.cs to see if a proxy is needed
            jobsApi = new JobsAPI(baseUrl, key, secret);
            //Initial sync to pull the current set of jobs (max limit of jobs obtainable is 100,000)
            InitialJobSync();
            //Set up a simple timer to keep pulling for jobs at a set interval
            SetUpTimer();
            //Press any key to exit
            Console.ReadKey();
        }

        /// <summary>
        /// Http request to the /jobs-sdk/jobs/{context} endpoint.
        /// 
        /// Make sure to review the documentation found on the developer's portal for additional information and limitations.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="limit"></param>
        /// <param name="properties"></param>
        /// <param name="devices"></param>
        /// <param name="startMarker"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> GetJobs(string context, int limit, string properties, string devices, long startMarker, string direction)
        {
            return await jobsApi.RetrieveJobs(context, limit, properties, devices, startMarker, direction);
        }

        /// <summary>
        /// Returns the reponse data from the /jobs-sdk/jobs/{context} call into a List
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static async Task<List<Job>> GetJobSet(HttpResponseMessage response)
        {
            //Check status of response. In cases where the frequency of the call rate is too high or the amount of calls exceeds
            //the set limit, a 429 Too Many Requests error will be returned.
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Unable to get job set due to: " + response.ReasonPhrase);
                return new List<Job>();
            }

            //Response data is returned as a stream. Reading through the stream will return json data
            Stream content = await response.Content.ReadAsStreamAsync();
            string jobsData;
            using (var decompress = new GZipStream(content, CompressionMode.Decompress))
            using (var sr = new StreamReader(decompress))
            {
                jobsData = sr.ReadToEnd();
            }

            //Deserialize json response data into Job list
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<Job> jobSet = JsonConvert.DeserializeObject<List<Job>>(jobsData, settings);

            return jobSet;
        }

        /// <summary>
        /// Get the latest set of jobs since the last marker value.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void GetLatestJobs(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Getting Latest Set of Job Data, Current Marker: " + startMarker);

            HttpResponseMessage response = GetJobs(JobContext.Job.Value, 1000, propertiesList, null, startMarker, "forward").Result;
            List<Job> newJobs = GetJobSet(response).Result;
            if (newJobs.Count > 0)
            {
                startMarker = newJobs.Last().marker;
                jobs = newJobs.Union(jobs, comparator).ToList(); //Union to remove duplicates (considered duplicate when deviceid and jobid are the same)
            }

            int dfeCount = 0;
            int pressCount = 0;
            int printRunCount = 0;
            int workEstimate = 0;

            //Simple usage of the set of data
            //Go thru and count the different types of jobs as well as get the total workTimeEstimate of jobs in the Print Queue 
            foreach (Job job in jobs)
            {
                if (job.jobType.Equals("DFE"))
                {
                    dfeCount++;
                }
                if (job.jobType.Equals("PRESS"))
                {
                    pressCount++;
                    if (job.location != null && job.location == "QUEUED")
                    {
                        workEstimate += job.jobWorkTimeEstimate;
                    }
                }
                if (job.jobType.Equals("PRINT_RUN"))
                {
                    printRunCount++;
                }
            }

            Console.WriteLine("Current Set of Jobs: " + jobs.Count + "\n" +
                "Current Marker: " + startMarker + "\n" +
                "Total DFE Jobs: " + dfeCount + "\n" +
                "Total Press Jobs: " + pressCount + ", Print Queue Work Estimate: " + workEstimate + "\n" +
                "Total Print Runs: " + printRunCount + "\n");
        }

        /// <summary>
        /// Get the initial set of job data for an account.
        /// Context is job so the set of data retrieved will include (dfe, press, printrun) context job data.
        /// 
        /// Note: The max amount of job data retrieved will be 100,000. The amount of calls where the 'limit'
        /// query parameter exceeds 1000 is 10 calls an hour. If your account will exceed this, consider taking
        /// note of the last marker of the job to make another 10 calls with a 10,000 limit after an hour.
        /// </summary>
        private static void InitialJobSync()
        {
            Console.WriteLine("Starting Initial Sync of Job Data");
            int limit = 10000;
            //Initial poll for the first set of 10000 jobs
            HttpResponseMessage response = GetJobs(JobContext.Job.Value, limit, propertiesList, null, startMarker, "forward").Result;
            List<Job> jobSet = GetJobSet(response).Result;
            jobs = jobSet;
            bool continueSync = (jobSet.Count == limit);
            startMarker = (jobSet.Count > 0) ? jobSet.Last().marker : startMarker;

            while (continueSync)
            {
                if (jobSet.Count > 0)
                {
                    startMarker = jobSet.Last().marker;
                }
                if (jobSet.Count < limit)
                {
                    continueSync = false;
                }
                else
                {
                    response = GetJobs(JobContext.Job.Value, limit, propertiesList, null, startMarker, "forward").Result;
                    jobSet = GetJobSet(response).Result;
                    jobs = jobs.Union(jobSet, comparator).ToList(); //Union to remove duplicates (considered duplicate when deviceid and jobid are the same)
                }
            }

            Console.WriteLine("Initial Sync Completed. Current Set of Jobs: " + jobs.Count);
        }

        /// <summary>
        /// Set up a simple timer to call the GetLatestJobs
        /// </summary>
        private static void SetUpTimer()
        {
            timer = new Timer(60000); //5 minutes in milliseconds
            timer.Elapsed += GetLatestJobs;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
    }
}
