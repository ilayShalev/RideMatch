using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Models
{
    public class Solution
    {
        // Properties for vehicles, score, etc.

        // Creates a deep clone of the solution
        public Solution Clone();

        // Gets the total distance of all routes
        public double GetTotalDistance();

        // Gets the total time of all routes
        public double GetTotalTime();

        // Gets the number of assigned passengers
        public int GetAssignedPassengerCount();

        // Gets the number of vehicles used
        public int GetUsedVehicleCount();
    }
}
