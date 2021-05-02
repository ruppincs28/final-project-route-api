using final_project_route_api.Models.HTTP;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace final_project_route_api.Models
{
    public class RouteCalculatorHelpers
    {
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

        public static int CalculateSecondsUntilStation(string json, Coordinates stopCoordinates)
        {
            JObject jObj = JObject.Parse(json);
            JArray jArr = (JArray)jObj["routes"].First["legs"].First["steps"];
            int returnVal = 0;

            foreach (var item in jArr.Children())
            {
                var itemProperties = item.Children<JProperty>();
                var travelMode = itemProperties.FirstOrDefault(x => x.Name == "travel_mode");
                var travelModeValue = travelMode.Value.ToString();
                var duration = itemProperties.FirstOrDefault(x => x.Name == "duration");
                var durationValue = Convert.ToInt32(duration.Value["value"].ToString());
                var startLocation = itemProperties.FirstOrDefault(x => x.Name == "start_location");

                if (Convert.ToDouble(startLocation.Value["lat"].ToString()) == stopCoordinates.Lat
                    && Convert.ToDouble(startLocation.Value["lng"].ToString()) == stopCoordinates.Lng)
                {
                    break;
                }

                returnVal += durationValue;
            }

            return returnVal;
        }

        public static List<Coordinates> GetStationsInRoute(string json)
        {
            JObject jObj = JObject.Parse(json);
            JArray jArr = (JArray)jObj["routes"].First["legs"].First["steps"];
            List<Coordinates> stations = new List<Coordinates>();

            foreach (var item in jArr.Children())
            {
                var itemProperties = item.Children<JProperty>();
                var travelMode = itemProperties.FirstOrDefault(x => x.Name == "travel_mode");
                var travelModeValue = travelMode.Value.ToString();

                if (travelModeValue == "TRANSIT")
                {
                    var startLocation = itemProperties.FirstOrDefault(x => x.Name == "start_location");
                    stations.Add(new Coordinates
                        (Convert.ToDouble(startLocation.Value["lat"].ToString()), Convert.ToDouble(startLocation.Value["lng"].ToString())));
                }
            }

            return stations;
        }

        public static int GetSecondsByCar(Coordinates origin, Coordinates dest, int secondsWithoutCar, string API_KEY)
        {
            string json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/directions/json" +
                $"?origin={origin.Lat},{origin.Lng}&destination={dest.Lat},{dest.Lng}&mode=driving&key={API_KEY}&language=en-US");
            JObject jObj = JObject.Parse(json);
            int secondsByCar = Convert.ToInt32(jObj["routes"].First["legs"].First["duration"]["value"].ToString());

            return secondsByCar;
        }

        public static int GetLengthOfRouteByCar(Coordinates origin, Coordinates dest, int secondsWithoutCar, string API_KEY)
        {
            return GetSecondsByCar(origin, dest, secondsWithoutCar, API_KEY);
        }

        public static bool IsBeneficialToDriveByCar(Coordinates origin, Coordinates dest, int secondsWithoutCar, string API_KEY)
        {
            return GetSecondsByCar(origin, dest, secondsWithoutCar, API_KEY) < secondsWithoutCar;
        }
    }
}