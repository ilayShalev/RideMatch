using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Models
{
    public class Passenger
    {
        // Properties for passenger details (ID, name, coordinates, etc.)

        // Calculates distance to a given location
        public double DistanceTo(double latitude, double longitude);

        // Creates a deep clone of the passenger
        public Passenger Clone();
   }
}
