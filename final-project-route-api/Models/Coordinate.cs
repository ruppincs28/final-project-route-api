using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace final_project_route_api.Models
{
    public class Coordinate
    {
        double lat;
        double lng;

        public Coordinate(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }

        public double Lat { get => lat; set => lat = value; }
        public double Lng { get => lng; set => lng = value; }
    }
}