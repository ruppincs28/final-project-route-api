using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace final_project_route_api.Models.HTTP
{
    public class HTTPHelpers
    {
        public static string SynchronizedRequest(string requestType, string url)
        {
            string res;

            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.Method = requestType;

            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                int statusCode = (int)response.StatusCode;
                using (Stream rs = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(rs))
                    {
                        res = sr.ReadToEnd();
                    }
                }
            }

            return res;
        }
    }
}