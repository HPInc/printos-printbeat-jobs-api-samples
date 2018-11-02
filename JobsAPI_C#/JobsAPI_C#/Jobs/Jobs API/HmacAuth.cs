using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Jobs
{
    class HmacAuth
    {
        private static string key;
        private static string secret;

        public HmacAuth(string key, string secret)
        {
            HmacAuth.key = key;
            HmacAuth.secret = secret;
        }

        public void CreateHmacHeaders(string method, string path, HttpClient client)
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            string stringToSign = method + " " + path + timestamp;
            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
            byte[] bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            string signature = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
            string auth = key + ":" + signature;

            client.DefaultRequestHeaders.Add("x-hp-hmac-authentication", auth);
            client.DefaultRequestHeaders.Add("x-hp-hmac-date", timestamp);
            client.DefaultRequestHeaders.Add("x-hp-hmac-algorithm", "SHA1");
        }

        public string getHmacAuthentication(string method, string path, string timestamp)
        {
            string stringToSign = method + " " + path + timestamp;
            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
            byte[] bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            string signature = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();

            return key + ":" + signature;
        }
    }
}
