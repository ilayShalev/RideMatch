using System;
using System.Collections.Generic;
using System.Linq;

namespace RideMatch.Core.Models
{
    public class Route
    {
        public int Id { get; set; }
        public Vehicle Vehicle { get; set; }
        public List<Passenger> Passengers { get; set; }
        public double TotalDistance { get; set; }
        public double TotalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }

        public Route()
        {
            Passengers = new List<Passenger>();
        }

        // Calculates the total distance of the route
        public double CalculateDistance()
        {
            if (Vehicle == null || Passengers.Count == 0)
                return 0;

            double totalDistance = 0;

            // Vehicle to first passenger
            totalDistance += Vehicle.DistanceTo(Passengers[0].Latitude, Passengers[0].Longitude);

            // Between passengers
            for (int i = 0; i < Passengers.Count - 1; i++)
            {
                totalDistance += Passengers[i].DistanceTo(Passengers[i + 1].Latitude, Passengers[i + 1].Longitude);
            }

            // Last passenger to destination (would need destination coordinates)
            // This is simplified here

            return totalDistance;
        }

        // Calculates the total time of the route
        public double CalculateTime()
        {
            // Assuming average speed of 30 km/h in urban areas
            double distance = CalculateDistance();
            return (distance / 30) * 60; // Convert to minutes
        }

        // Calculates estimated pickup times based on target arrival
        public void CalculatePickupTimes(DateTime targetArrivalTime)
        {
            if (Vehicle == null || Passengers.Count == 0)
                return;

            // Set arrival time
            ArrivalTime = targetArrivalTime;

            // Calculate total time in minutes
            double totalTime = CalculateTime();

            // Set departure time
            DepartureTime = targetArrivalTime.AddMinutes(-totalTime);

            // Calculate time to first passenger
            double timeToFirstPassenger = (Vehicle.DistanceTo(Passengers[0].Latitude, Passengers[0].Longitude) / 30) * 60;

            // Set pickup time for first passenger
            Passengers[0].PickupTime = DepartureTime.Value.AddMinutes(timeToFirstPassenger);

            // Calculate pickup times for remaining passengers
            for (int i = 1; i < Passengers.Count; i++)
            {
                double timeBetweenPassengers = (Passengers[i - 1].DistanceTo(Passengers[i].Latitude, Passengers[i].Longitude) / 30) * 60;
                Passengers[i].PickupTime = Passengers[i - 1].PickupTime.Value.AddMinutes(timeBetweenPassengers);
            }
        }

        // Validates that the route meets all constraints
        public bool Validate()
        {
            // Check if vehicle exists
            if (Vehicle == null)
                return false;

            // Check if passengers exist
            if (Passengers == null || Passengers.Count == 0)
                return false;

            // Check if vehicle has enough capacity
            if (Passengers.Count > Vehicle.Capacity)
                return false;

            // More validation logic could be added here

            return true;
        }
    }
}