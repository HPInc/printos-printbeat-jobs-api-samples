using System;
using System.Collections.Generic;

namespace Jobs
{
    class HistoricJob
    {
        public string deviceId { get; set; }
        public int impressions { get; set; }
        public int impressions1Color { get; set; }
        public int impressions2Colors { get; set; }
        public int impressionsNColors { get; set; }
        public string impressionsType { get; set; }
        public Ink inks { get; set; }
        public string inkUnits { get; set; }
        public string jobCompleteTime { get; set; }
        public string jobName { get; set; }
        public string jobSubmitTime { get; set; }
        public long marker { get; set; }
        public Substrate substrates { get; set; }
        public string substrateUnits { get; set; }
    }

    class HistoricJobComparator : IEqualityComparer<HistoricJob>
    {
        public bool Equals(HistoricJob x, HistoricJob y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.marker == y.marker;
        }

        public int GetHashCode(HistoricJob job)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(job, null)) return 0;

            int hashJobId = job.marker.GetHashCode();

            //Calculate the hash code for the product.
            return hashJobId;
        }
    }
}