using RideMatch.Utilities.Geo;
using System;

namespace RideMatch.Core.Models
{
    public class Passenger
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? PickupTime { get; set; }

        // Calculates distance to a given location
        public double DistanceTo(double latitude, double longitude)
        {
            return DistanceCalculator.CalculateDistance(
                Latitude, Longitude, latitude, longitude);
        }

        // Creates a deep clone of the passenger
        public Passenger Clone()
        {
            return new Passenger
            {
                Id = Id,
                UserId = UserId,
                Name = Name,
                Latitude = Latitude,
                Longitude = Longitude,
                Address = Address,
                IsAvailable = IsAvailable,
                PickupTime = PickupTime
            };
        }
    }
}