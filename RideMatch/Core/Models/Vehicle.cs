using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Models
{
    public class Vehicle
    {
        // Properties for vehicle details (ID, capacity, driver, coordinates, etc.)

        // Calculates distance to a given location
        public double DistanceTo(double latitude, double longitude);

        // Checks if the vehicle has enough capacity for a given number of passengers
        public bool HasCapacity(int requiredSeats);

        // Creates a deep clone of the vehicle
        public Vehicle Clone();
   }
}
