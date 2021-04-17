using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace final_project_route_api.Models.HTTP
{
    public class JSONHelpers
    {
        public static string SerializeObject<T>(T objectToSend)
        {
            var settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            return JsonConvert.SerializeObject(objectToSend, settings);
        }

        public static T DeSerializerResponse<T>(string sReader, string path = null)
        {
            var settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };

            if (!string.IsNullOrEmpty(path))
            {
                sReader = JObject.Parse(sReader).SelectToken(path).ToString();
            }

            var result = JsonConvert.DeserializeObject<T>(sReader, settings);
            return result;
        }

        public static T DeSerializerResponseXml<T>(string sReader)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(new StringReader(sReader));
        }

        public static T CloneObject<T>(object toCopy)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(toCopy));
        }

        public static bool JsonCompare<T>(T obj, T another, params string[] ignore)
        {
            if (ReferenceEquals(obj, another))
            {
                return true;
            }

            if (obj == null || another == null)
            {
                return false;
            }

            var objJson = JsonConvert.SerializeObject(obj);
            var anotherJson = JsonConvert.SerializeObject(another);

            return objJson == anotherJson;
        }
    }
}
