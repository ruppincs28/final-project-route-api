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
        Coordinates sourceCoordinates;
        Coordinates destCoordinates;
        string steps;
        bool isCarDriveBeneficial;

        public RouteCalculator(string name, Coordinates sourceCoordinates, Coordinates destCoordinates, string steps, bool isCarDriveBeneficial)
        {
            Name = name;
            SourceCoordinates = sourceCoordinates;
            DestCoordinates = destCoordinates;
            Steps = steps;
            IsCarDriveBeneficial = isCarDriveBeneficial;
        }

        public string Name { get => name; set => name = value; }
        public Coordinates SourceCoordinates { get => sourceCoordinates; set => sourceCoordinates = value; }
        public Coordinates DestCoordinates { get => destCoordinates; set => destCoordinates = value; }
        public string Steps { get => steps; set => steps = value; }
        public bool IsCarDriveBeneficial { get => isCarDriveBeneficial; set => isCarDriveBeneficial = value; }

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

        public static Coordinates ExtractFirstStationsCoordinates(string json)
        {
            JObject jObj = JObject.Parse(json);
            JArray jArr = (JArray)jObj["routes"].First["legs"].First["steps"];
            Coordinates returnVal = new Coordinates(0, 0);

            foreach (var item in jArr.Children())
            {
                var itemProperties = item.Children<JProperty>();
                var travelMode = itemProperties.FirstOrDefault(x => x.Name == "travel_mode");
                var travelModeValue = travelMode.Value.ToString();

                if (travelModeValue == "TRANSIT")
                {
                    var startLocation = itemProperties.FirstOrDefault(x => x.Name == "start_location");
                    returnVal = new Coordinates(
                        Convert.ToDouble(startLocation.Value["lat"].ToString()), Convert.ToDouble(startLocation.Value["lng"].ToString()));
                }
            }

            return returnVal;
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

                if (Convert.ToInt32(startLocation.Value["lat"].ToString()) == stopCoordinates.Lat
                    && Convert.ToInt32(startLocation.Value["lng"].ToString()) == stopCoordinates.Lng)
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

        public static int GetLengthOfRouteByCar(Coordinates origin, Coordinates dest, int secondsWithoutCar, string API_KEY)
        {
            string json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/directions/json" +
                    $"?origin={origin.Lat},{origin.Lng}&destination={dest.Lat},{dest.Lng}&mode=driving&key={API_KEY}&language=en-US");
            JObject jObj = JObject.Parse(json);
            int secondsByCar = Convert.ToInt32(jObj["routes"].First["legs"].First["duration"]["value"].ToString());

            return secondsByCar;
        }

        public static bool IsBeneficialToDriveByCar(Coordinates origin, Coordinates dest, int secondsWithoutCar, string API_KEY)
        {
            string json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/directions/json" +
                    $"?origin={origin.Lat},{origin.Lng}&destination={dest.Lat},{dest.Lng}&mode=driving&key={API_KEY}&language=en-US");
            JObject jObj = JObject.Parse(json);
            int secondsByCar = Convert.ToInt32(jObj["routes"].First["legs"].First["duration"]["value"].ToString());

            return secondsByCar < secondsWithoutCar;
        }

        public static List<RouteCalculator> Calculate(ClientRouteCalculatorRequest rcr)
        {
            string API_KEY = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["GOOGLE_API_KEY"]);
            List<RouteCalculator> resultList = new List<RouteCalculator>();

            foreach (string company in rcr.CompaniesWithAddresses)
            {
                string companyNameWithStateEscaped = Uri.EscapeUriString(company);

                /* Extract exact address from company + state combination */
                string json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?" +
                    $"input={companyNameWithStateEscaped}&inputtype=textquery&language=en-US&fields=all&key={API_KEY}");
                string exactAddress = ExtractFormattedAddress(json);
                string exactAddressEscaped = Uri.EscapeUriString(exactAddress);

                /* Extract latitude, longitude from an address */
                json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/geocode/json?" +
                    $"address={exactAddressEscaped}&key={API_KEY}");
                Coordinates destCoordinates = ExtractLatLng(json);
                Coordinates originCoordinates = new Coordinates(rcr.Coordinates.Lat, rcr.Coordinates.Lng);

                // Get Route
                json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/directions/json" +
                    $"?origin={originCoordinates.Lat},{originCoordinates.Lng}&destination={destCoordinates.Lat},{destCoordinates.Lng}&mode=transit&key={API_KEY}&language=en-US&departure_time=1619897612");

                /* Calculate whether it is beneficial to use a car to drive to one of the stations */
                // Loop over to get all station coordinates
                List<Coordinates> stationsInRoute = GetStationsInRoute(json);
                // Calculate route to station via private vehicle
                int minLengthId = -1, minLengthRoute = Int32.MaxValue;

                for (int i = 0; i < stationsInRoute.Count; i++)
                {
                    Coordinates c = stationsInRoute[i];
                    if (IsBeneficialToDriveByCar(originCoordinates, c, CalculateSecondsUntilStation(json, c), API_KEY))
                    {
                        int seconds = GetLengthOfRouteByCar(originCoordinates, c, CalculateSecondsUntilStation(json, c), API_KEY);
                        if (seconds <= minLengthRoute)
                        {
                            minLengthRoute = seconds;
                            minLengthId = i;
                        }
                    }
                }

                Coordinates firstStationCoordinates = ExtractFirstStationsCoordinates(json);
                bool isBeneficial = false;

                RouteCalculator route = new RouteCalculator(
                    exactAddress, destCoordinates, firstStationCoordinates, "ABC", isBeneficial);
                resultList.Add(route);
            }

            return resultList;
        }
    }
}