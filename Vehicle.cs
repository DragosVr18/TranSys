using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranSys
{
    public class Vehicle
    {
        public int Id { get; }
        public string Model { get; }
        public string Type { get; }
        public string AssignedLine { get; set; }
        public List<OffsetTime> TourDepartures { get; set; }
        public List<OffsetTime> BackDepartures { get; set; }

        public Vehicle(int id, string model, string type, string assignedLine, List<OffsetTime> tourDepartures, List<OffsetTime> backDepartures)
        {
            Id = id;
            Model = model;
            Type = type;
            AssignedLine = assignedLine;
            TourDepartures = tourDepartures;
            BackDepartures = backDepartures;
        }

        public Vehicle(int id, string model, string type)
        {
            Id = id;
            Model = model;
            Type = type;
            AssignedLine = "";
            TourDepartures = new List<OffsetTime>();
            BackDepartures = new List<OffsetTime>();
        }
    }
}
