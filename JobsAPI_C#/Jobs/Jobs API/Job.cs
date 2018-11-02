using System;
using System.Collections.Generic;

namespace Jobs
{
    class Job
    {
        public string deviceId { get; set; }
        public string jobId { get; set; }
        public string jobName { get; set; }
        public string jobProgress { get; set; }
        public string jobSubmitTime { get; set; }
        public string jobType { get; set; }
        public int jobWorkTimeEstimate { get; set; }
        public string location { get; set; }
        public long marker { get; set; }
        public string ticketTemplate { get; set; }
    }

    class JobComparator : IEqualityComparer<Job>
    {
        public bool Equals(Job x, Job y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.jobId == y.jobId && x.deviceId == y.deviceId;
        }

        public int GetHashCode(Job job)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(job, null)) return 0;

            int hashJobId = job.jobId.GetHashCode();
            int hashDeviceId = job.deviceId.GetHashCode();

            //Calculate the hash code for the product.
            return hashJobId ^ hashDeviceId;
        }
    }
}
