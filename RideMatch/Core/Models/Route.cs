using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Models
{
    public class Route
    {
        // Properties for route details (vehicle, passengers, distance, time, etc.)

        // Calculates the total distance of the route
        public double CalculateDistance();

        // Calculates the total time of the route
        public double CalculateTime();

        // Calculates estimated pickup times based on target arrival
        public void CalculatePickupTimes(DateTime targetArrivalTime);

        // Validates that the route meets all constraints
        public bool Validate();
    }
}
    