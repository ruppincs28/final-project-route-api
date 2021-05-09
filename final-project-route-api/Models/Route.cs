using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace final_project_route_api.Models
{
    public class Route
    {
        string name;
        Coordinates sourceCoordinates;
        Coordinates destCoordinates;
        Coordinates privateVehicleCoordinates;
        bool isCarDriveBeneficial;
        string carDriveJustification;

        public Route(string name, Coordinates sourceCoordinates, 
            Coordinates destCoordinates, Coordinates privateVehicleCoordinates, bool isCarDriveBeneficial, string carDriveJustification)
        {
            Name = name;
            SourceCoordinates = sourceCoordinates;
            DestCoordinates = destCoordinates;
            PrivateVehicleCoordinates = privateVehicleCoordinates;
            IsCarDriveBeneficial = isCarDriveBeneficial;
            CarDriveJustification = carDriveJustification;
        }

        public string Name { get => name; set => name = value; }
        public Coordinates SourceCoordinates { get => sourceCoordinates; set => sourceCoordinates = value; }
        public Coordinates DestCoordinates { get => destCoordinates; set => destCoordinates = value; }
        public Coordinates PrivateVehicleCoordinates { get => privateVehicleCoordinates; set => privateVehicleCoordinates = value; }
        public bool IsCarDriveBeneficial { get => isCarDriveBeneficial; set => isCarDriveBeneficial = value; }
        public string CarDriveJustification { get => carDriveJustification; set => carDriveJustification = value; }
    }
}