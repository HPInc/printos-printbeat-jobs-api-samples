using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Jobs
{
    class JobsAPI
    {
        private static string baseUrl;
        private static HmacAuth hmacAuth;

        //Uncomment Proxy/UseProxy lines below if using a web proxy
        private HttpClientHandler handler = new HttpClientHandler()
        {
            //Proxy = new WebProxy("web-proxy.corp.hp.com:8080"),
            //UseProxy = true,
        };

        public JobsAPI(string baseUrl, string key, string secret)
        {
            JobsAPI.baseUrl = baseUrl;
            hmacAuth = new HmacAuth(key, secret);
        }

        public async Task<HttpResponseMessage> RetrieveJobs(string context, int limit, string properties, string devices, long startMarker, string direction)
        {
            using (var client = new HttpClient(handler, false))
            {
                string path = "/jobs-sdk/jobs/" + context;
                UriBuilder fullPath = new UriBuilder(baseUrl + path);

                var query = HttpUtility.ParseQueryString(fullPath.Query);
                query["limit"] = limit.ToString();
                if (properties != null) query["properties"] = properties;
                if (devices != null) query["devices"] = devices;
                query["startMarker"] = startMarker.ToString();
                if (direction != null) query["direction"] = direction;
                fullPath.Query = query.ToString();

                hmacAuth.CreateHmacHeaders("GET", path, client);

                HttpResponseMessage response = await client.GetAsync(fullPath.ToString());
                return response;
            }
        }

        public async Task<HttpResponseMessage> RetrievePropertySpecs(string context)
        {
            using (var client = new HttpClient(handler, false))
            {
                string path = "/jobs-sdk/propertyspecs";
                string contextParam = (context != null) ? "?context=" + context : "";
                
                string fullPath = baseUrl + path + contextParam;

                hmacAuth.CreateHmacHeaders("GET", path, client);

                HttpResponseMessage response = await client.GetAsync(fullPath);
                return response;
            }
        }
    }
}
