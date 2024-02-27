using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranSys
{
    public class Line
    {
        private string _name;
        private string _vehicleType;
        private string _description;
        private string _begin;
        private string _back;

        public string Name { get { return _name; } }
        public string VehicleType { get { return _vehicleType; } }
        public string Description { get { return _description; } }
        public string Begin { get { return _begin; } }
        public string Back { get { return _back; } }

        public Line(string name, string vehicleType, string description, string begin, string back)
        {
            _name = name;
            _vehicleType = vehicleType;
            _description = description;
            _begin = begin;
            _back = back;
        }
    }

    public class LineValidator
    {
        public static void validateLine(string name, string vehicleType, string description, string begin, string back)
        {
            string err = "";
            if (name.Length == 0)
                err += "Line name cannot be empty\n";
            if (vehicleType != "bus" && vehicleType != "trolley" && vehicleType != "tram")
                err += "Line must have a vehicle type\n";
            if (description.Length == 0)
                err += "Line must have a description\n";
            if (begin.Length == 0)
                err += "Line must have a tour station\n";
            if (back.Length == 0)
                err += "Line must have a back station\n";
            if (err != "")
            {
                err.Remove(err.Length - 1, 1);
                throw new Exception(err);
            }
        }
    }
}
