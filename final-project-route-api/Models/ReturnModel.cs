using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace final_project_route_api.Models
{
    public class ReturnModel
    {
        string name;
        Coordinate coordinate;
        string steps;

        public ReturnModel(string name, Coordinate coordinate, string steps)
        {
            Name = name;
            Coordinate = coordinate;
            Steps = steps;
        }

        public string Name { get => name; set => name = value; }
        public Coordinate Coordinate { get => coordinate; set => coordinate = value; }
        public string Steps { get => steps; set => steps = value; }

        public static string ExtractFormattedAddress(string json)
        {
            JObject jo = JObject.Parse(json);

            return jo["candidates"].First["formatted_address"].ToString();
        }

        public static Coordinate ExtractLatLng(string json)
        {
            JObject jo = JObject.Parse(json);

            double lat = Convert.ToDouble(jo["results"].First["geometry"]["location"]["lat"].ToString());
            double lng = Convert.ToDouble(jo["results"].First["geometry"]["location"]["lng"].ToString());

            return new Coordinate(lat, lng);
        }

        public static string ExtractSteps(string json)
        {
            JObject jObj = JObject.Parse(json);
            JArray jArr = (JArray)jObj["routes"].First["legs"].First["steps"];

            string returnVal = "";

            foreach (var item in jArr.Children())
            {
                var itemProperties = item.Children<JProperty>();

                var myElement = itemProperties.FirstOrDefault(x => x.Name == "duration");
                var myElementValue = myElement.Value["text"];

                returnVal += myElementValue + " - ";
            }

            return returnVal.Substring(0, returnVal.Length - 3);
        }
    }
}