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
    /// In this sample we are showing the basics of establishing a local view of historic jobs in an account and updating them every hour or 24 hours depending on the data retrieved.
    /// The example shows the pulling of the historical jobs and sets a timer until the next time to pull
    /// Once you have this view of your jobs, you can analyze the data as you see fit. 
    /// In this example only the current startMarker and total amount of jobs in-memory are outputed. You may want to persist your view in a formal database.
    class RetrieveHistoricJobs
    {
        //Access Credentials
        private static string baseUrl = "https://printos.api.hp.com/jobs-service";        //use for production account
        //private static string baseUrl = "https://stage.printos.api.hp.com/jobs-service";    //use for staging account

        private static string key = "";   //Enter PrintOS key here
        private static string secret = "";    //Enter PrintOS secret here

        private static HistoricJobComparator comparator = new HistoricJobComparator();
        private static List<HistoricJob> jobs = new List<HistoricJob>();
        private static JobsAPI jobsApi;
        private static Timer timer;

        //Set of properties to ask for, if there are any additional properties needed make sure to add them to HistoricJob.cs
        //Also be sure to review what the available set of properties are available in each context
        private static string propertiesList = "deviceId,impressions,impressions1Color,impressions2Colors,impressionsNColors,impressionsType,inks,inkUnits,jobCompleteTime,jobName,jobSubmitTime,marker,substrates,substrateUnits";
        private static long startMarker = 0;
        private static int limit = 10000;

        static void Main(string[] args)
        {
            //Forcing TLS to 1.2 prevents an issue with older .NET frameworks defaulting to 1.0 which is not supported.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //Review the JobsAPI.cs to see if a proxy is needed
            jobsApi = new JobsAPI(baseUrl, key, secret);
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
        private static async Task<List<HistoricJob>> GetJobSet(HttpResponseMessage response)
        {
            //Check status of response. In cases where the frequency of the call rate is too high or the amount of calls exceeds
            //the set limit, a 429 Too Many Requests error will be returned.
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Unable to get job set due to: " + response.ReasonPhrase);
                return new List<HistoricJob>();
            }

            //Response data is returned as a stream. Reading through the stream will return json data
            Stream content = await response.Content.ReadAsStreamAsync();
            string jobsData;
            //using (var decompress = new GZipStream(content, CompressionMode.Decompress))
            using (var sr = new StreamReader(content))
            {
                jobsData = sr.ReadToEnd();
            }

            //Deserialize json response data into Job list
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<HistoricJob> jobSet = JsonConvert.DeserializeObject<List<HistoricJob>>(jobsData, settings);

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

            //Retrieving the first set of 10000 jobs since the last known marker value
            HttpResponseMessage response = GetJobs(JobContext.Historic.Value, limit, propertiesList, null, startMarker, "forward").Result;
            List<HistoricJob> jobSet = GetJobSet(response).Result;
            jobs = jobSet.Union(jobs, comparator).ToList(); //Union to remove any duplicates
            bool continueSync = (jobSet.Count == limit);
            startMarker = (jobSet.Count > 0) ? jobSet.Last().marker : startMarker;

            while (continueSync)
            {
                if (jobSet.Count > 0)
                {
                    startMarker = jobSet.Last().marker; //Only update the marker if the last response returned any jobs
                }
                if (jobSet.Count < limit)
                {
                    continueSync = false; //No longer need to sync up jobs as the result set has returned less than 10000 jobs (either due to an error or success)
                }
                else
                {
                    response = GetJobs(JobContext.Historic.Value, limit, propertiesList, null, startMarker, "forward").Result;
                    jobSet = GetJobSet(response).Result;
                    jobs = jobSet.Union(jobs, comparator).ToList(); //Union to remove any duplicates
                }
            }
            //At this point the jobs List has all new print run jobs since the last time through this routine. 
            //Here you would filter or sort through the data to get the information of interest.

            Console.WriteLine("Obtained Latest Set of Jobs. Current Set: " + jobs.Count + "\n" +
                "Current Marker: " + startMarker + "\n");

            //Based on the status of the last call, determine what interval the timer will need to be to next trigger the retrieval of the latest jobs
            //An error will usually be due to hitting the limit of permitted calls each hour, so we will set the interval to being an hour (in milliseconds).
            //
            //If it was successful we can assume that all the data has been retrieved and the data set is up to date, so the interval will be set to 24 hours
            //because the data in the historic context is only updated once a day.
            int interval = (response.IsSuccessStatusCode) ? 86400000 : 3600000;
            UpdateTimer(interval);
        }

        /// <summary>
        /// Set up a simple timer to call the GetLatestJobs
        /// </summary>
        private static void SetUpTimer()
        {
            timer = new Timer(1000); //Set timer to initially trigger after a second. 
            timer.Elapsed += GetLatestJobs;
            timer.AutoReset = false;
            timer.Enabled = true;
        }

        /// <summary>
        /// Update and re-enable timer to trigger at the specified interval
        /// </summary>
        /// <param name="interval"></param>
        private static void UpdateTimer(int interval)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(interval);
            Console.WriteLine("Setting timer to retrieve next set of jobs in " + time.TotalHours + " hours");
            timer.Enabled = false;
            timer.Interval = interval;
            timer.Enabled = true;
        }
    }
}
