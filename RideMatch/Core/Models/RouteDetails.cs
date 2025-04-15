using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Models
{
    public class RouteDetails
    {
        // Properties for vehicle ID, total distance, total time, stop details, etc.

        // Adds a stop to the route
        public void AddStop(int passengerId, string passengerName, double distanceFromPrevious, double timeFromPrevious);

        // Calculates cumulative distances and times
        public void CalculateCumulatives();

        // Gets total distance
        public double GetTotalDistance();

        // Gets total time
        public double GetTotalTime();
    }
}
