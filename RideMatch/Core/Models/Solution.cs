using System;
using System.Collections.Generic;
using System.Linq;

namespace RideMatch.Core.Models
{
    public class Solution
    {
        public List<Vehicle> Vehicles { get; set; }
        public double Score { get; set; }

        public Solution()
        {
            Vehicles = new List<Vehicle>();
            Score = 0;
        }

        // Creates a deep clone of the solution
        public Solution Clone()
        {
            var clone = new Solution
            {
                Score = Score
            };

            // Clone vehicles
            foreach (var vehicle in Vehicles)
            {
                clone.Vehicles.Add(vehicle.Clone());
            }

            return clone;
        }

        // Gets the total distance of all routes
        public double GetTotalDistance()
        {
            return Vehicles.Sum(v => v.TotalDistance);
        }

        // Gets the total time of all routes
        public double GetTotalTime()
        {
            return Vehicles.Sum(v => v.TotalTime);
        }

        // Gets the number of assigned passengers
        public int GetAssignedPassengerCount()
        {
            return Vehicles.Sum(v => v.Passengers.Count);
        }

        // Gets the number of vehicles used
        public int GetUsedVehicleCount()
        {
            return Vehicles.Count(v => v.Passengers.Count > 0);
        }
    }
}