using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace final_project_route_api.Models
{
    public class ClientRouteCalculatorRequest
    {
        List<string> companiesWithAddresses;
        Coordinate coordinate;

        public ClientRouteCalculatorRequest(List<string> companiesWithAddresses, Coordinate coordinate)
        {
            CompaniesWithAddresses = companiesWithAddresses;
            Coordinate = coordinate;
        }

        public List<string> CompaniesWithAddresses { get => companiesWithAddresses; set => companiesWithAddresses = value; }
        public Coordinate Coordinate { get => coordinate; set => coordinate = value; }
    }
}