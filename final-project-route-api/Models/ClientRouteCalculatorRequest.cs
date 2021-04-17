using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace final_project_route_api.Models
{
    public class ClientRouteCalculatorRequest
    {
        List<string> companiesWithAddresses;
        Coordinates coordinates;

        public ClientRouteCalculatorRequest(List<string> companiesWithAddresses, Coordinates coordinates)
        {
            CompaniesWithAddresses = companiesWithAddresses;
            Coordinates = coordinates;
        }

        public List<string> CompaniesWithAddresses { get => companiesWithAddresses; set => companiesWithAddresses = value; }
        public Coordinates Coordinates { get => coordinates; set => coordinates = value; }
    }
}