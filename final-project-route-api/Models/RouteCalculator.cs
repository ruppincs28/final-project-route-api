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
        public static List<Route> Calculate(ClientRouteCalculatorRequest rcr)
        {
            string API_KEY = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["GOOGLE_API_KEY"]);
            List<Route> resultList = new List<Route>();

            foreach (string company in rcr.CompaniesWithAddresses)
            {
                string companyNameWithStateEscaped = Uri.EscapeUriString(company);

                /* Extract exact address from company + state combination */
                string json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?" +
                    $"input={companyNameWithStateEscaped}&inputtype=textquery&language=en-US&fields=all&key={API_KEY}");
                string exactAddress = RouteCalculatorHelpers.ExtractFormattedAddress(json);
                string exactAddressEscaped = Uri.EscapeUriString(exactAddress);

                /* Extract latitude, longitude from an address */
                json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/geocode/json?" +
                    $"address={exactAddressEscaped}&key={API_KEY}");
                Coordinates destCoordinates = RouteCalculatorHelpers.ExtractLatLng(json);
                Coordinates originCoordinates = new Coordinates(rcr.Coordinates.Lat, rcr.Coordinates.Lng);

                // Get Route
                json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/directions/json" +
                    $"?origin={originCoordinates.Lat},{originCoordinates.Lng}&destination={destCoordinates.Lat},{destCoordinates.Lng}&mode=transit&key={API_KEY}&language=en-US");

                /* Calculate whether it is beneficial to use a car to drive to one of the stations */
                // Loop over to get all station coordinates
                List<Coordinates> stationsInRoute = RouteCalculatorHelpers.GetStationsInRoute(json);
                // Calculate route to station via private vehicle
                int minLengthId = -1, minLengthRoute = int.MaxValue, differenceCarVsPublic = -1;

                for (int i = 0; i < stationsInRoute.Count; i++)
                {
                    Coordinates c = stationsInRoute[i];
                    int secondsUntilStationNoCar = RouteCalculatorHelpers.CalculateSecondsUntilStation(json, c);
                    if (RouteCalculatorHelpers.IsBeneficialToDriveByCar(originCoordinates, c, secondsUntilStationNoCar, API_KEY))
                    {
                        int seconds = RouteCalculatorHelpers.GetLengthOfRouteByCar(originCoordinates, c, secondsUntilStationNoCar, API_KEY);
                        if (seconds < minLengthRoute)
                        {
                            minLengthRoute = seconds;
                            minLengthId = i;
                            differenceCarVsPublic = secondsUntilStationNoCar - seconds;
                        }
                    }
                }

                bool isBeneficial;
                Coordinates prvVehicleCoords;
                string carDriveJustification = "";
                try
                {
                    prvVehicleCoords = new Coordinates(stationsInRoute[minLengthId].Lat, stationsInRoute[minLengthId].Lng);
                    isBeneficial = true;
                    TimeSpan time = TimeSpan.FromSeconds(differenceCarVsPublic);
                    string timeStr = time.ToString(@"hh\:mm\:ss");
                    carDriveJustification = $"Driving with a private vehicle to 'PrivateVehicleCoordinates' " +
                        $"and only then beginning the public transport journey will save {timeStr}";
                } catch(ArgumentOutOfRangeException oorException)
                {
                    prvVehicleCoords = new Coordinates(0, 0);
                    isBeneficial = false;
                    Console.WriteLine(oorException.ToString());
                }

                Route route = new Route(
                    exactAddress, originCoordinates, destCoordinates, prvVehicleCoords, isBeneficial, carDriveJustification);
                resultList.Add(route);
            }

            return resultList;
        }
    }
}