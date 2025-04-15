using RideMatch.Utilities.Geo;
using System;
using System.Collections.Generic;

namespace RideMatch.Core.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DriverName { get; set; }
        public int Capacity { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public bool IsAvailable { get; set; }
        public List<Passenger> Passengers { get; set; }
        public double TotalDistance { get; set; }
        public double TotalTime { get; set; }

        public Vehicle()
        {
            Passengers = new List<Passenger>();
        }

        // Calculates distance to a given location
        public double DistanceTo(double latitude, double longitude)
        {
            return DistanceCalculator.CalculateDistance(
                Latitude, Longitude, latitude, longitude);
        }

        // Checks if the vehicle has enough capacity for a given number of passengers
        public bool HasCapacity(int requiredSeats)
        {
            return Passengers.Count + requiredSeats <= Capacity;
        }

        // Creates a deep clone of the vehicle
        public Vehicle Clone()
        {
            var clone = new Vehicle
            {
                Id = Id,
                UserId = UserId,
                DriverName = DriverName,
                Capacity = Capacity,
                Latitude = Latitude,
                Longitude = Longitude,
                Address = Address,
                IsAvailable = IsAvailable,
                TotalDistance = TotalDistance,
                TotalTime = TotalTime
            };

            // Clone passengers
            foreach (var passenger in Passengers)
            {
                clone.Passengers.Add(passenger.Clone());
            }

            return clone;
        }
    }
}