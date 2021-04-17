using final_project_route_api.Models.HTTP;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace final_project_route_api.Models
{
    public class RouteCalculator
    {
        string name;
        Coordinates coordinates;
        string steps;

        public RouteCalculator(string name, Coordinates coordinates, string steps)
        {
            Name = name;
            Coordinates = coordinates;
            Steps = steps;
        }

        public string Name { get => name; set => name = value; }
        public Coordinates Coordinates { get => coordinates; set => coordinates = value; }
        public string Steps { get => steps; set => steps = value; }

        public static string ExtractFormattedAddress(string json)
        {
            JObject jo = JObject.Parse(json);

            return jo["candidates"].First["formatted_address"].ToString();
        }

        public static Coordinates ExtractLatLng(string json)
        {
            JObject jo = JObject.Parse(json);

            double lat = Convert.ToDouble(jo["results"].First["geometry"]["location"]["lat"].ToString());
            double lng = Convert.ToDouble(jo["results"].First["geometry"]["location"]["lng"].ToString());

            return new Coordinates(lat, lng);
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

        public static List<RouteCalculator> Calculate(ClientRouteCalculatorRequest rcr)
        {
            string API_KEY = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["GOOGLE_API_KEY"]);
            List<RouteCalculator> resultList = new List<RouteCalculator>();

            foreach (string company in rcr.CompaniesWithAddresses)
            {
                string companyNameWithStateEscaped = Uri.EscapeUriString(company);

                // Extract exact address from company + state combination
                string json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?" +
                    $"input={companyNameWithStateEscaped}&inputtype=textquery&language=en-US&fields=all&key={API_KEY}");
                string exactAddress = ExtractFormattedAddress(json);
                string exactAddressEscaped = Uri.EscapeUriString(exactAddress);

                // Extract latitude, longitude from an address
                json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/geocode/json?" +
                    $"address={exactAddressEscaped}&key={API_KEY}");
                Coordinates coordinates = ExtractLatLng(json);

                // Get Route
                json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/directions/json" +
                    $"?origin={rcr.Coordinates.Lat},{rcr.Coordinates.Lng}&destination={coordinates.Lat},{coordinates.Lng}&mode=transit&key={API_KEY}&language=en-US");
                string steps = ExtractSteps(json);

                RouteCalculator route = new RouteCalculator(exactAddress, coordinates, steps);
                resultList.Add(route);
            }

            return resultList;
        }
    }
}