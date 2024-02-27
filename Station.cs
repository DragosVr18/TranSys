using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranSys
{
    public class Station
    {
        public string _name { get; set; }
        public List<string> _vehicleTypes { get; set; }
        public int _capacity { get; set; }
        public double _long { get; set; }
        public double _lat { get; set; }

        public Station(string name, List<string> vehicleTypes, int capacity, double @long, double lat)
        {
            _name = name;
            _vehicleTypes = vehicleTypes;
            _capacity = capacity;
            _long = @long;
            _lat = lat;
        }
    }

    internal class StationValidator
    {
        public static void validateStation(string name, List<string> vehicleTypes, int capacity, double @long, double lat)
        {
            string errmsg = "";
            if (name.Length == 0)
                errmsg += "Station name cannot be empty\n";
            if (vehicleTypes.Count == 0)
                errmsg += "Station must support at least one type of vehicle\n";
            if (capacity <= 0)
                errmsg += "Capacity must be a positive value\n";
            if (@long <= 0 || lat <= 0)
                errmsg += "Station must have coordinates\n";
            if (errmsg != "")
            {
                errmsg.Remove(errmsg.Length - 1, 1);
                throw new Exception(errmsg);
            }
        }
    }
}
