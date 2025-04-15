using System;
using System.Collections.Generic;
using System.Linq;

namespace RideMatch.Core.Models
{
    public class RouteDetails
    {
        public int VehicleId { get; set; }
        public double TotalDistance { get; set; }
        public double TotalTime { get; set; }
        public List<StopDetail> StopDetails { get; set; }

        public RouteDetails()
        {
            StopDetails = new List<StopDetail>();
        }

        // Adds a stop to the route
        public void AddStop(int passengerId, string passengerName, double distanceFromPrevious, double timeFromPrevious)
        {
            StopDetails.Add(new StopDetail
            {
                StopNumber = StopDetails.Count + 1,
                PassengerId = passengerId,
                PassengerName = passengerName,
                DistanceFromPrevious = distanceFromPrevious,
                TimeFromPrevious = timeFromPrevious
            });
        }

        // Calculates cumulative distances and times
        public void CalculateCumulatives()
        {
            double cumulativeDistance = 0;
            double cumulativeTime = 0;

            foreach (var stop in StopDetails)
            {
                cumulativeDistance += stop.DistanceFromPrevious;
                cumulativeTime += stop.TimeFromPrevious;

                stop.CumulativeDistance = cumulativeDistance;
                stop.CumulativeTime = cumulativeTime;
            }

            TotalDistance = cumulativeDistance;
            TotalTime = cumulativeTime;
        }

        // Gets total distance
        public double GetTotalDistance()
        {
            return TotalDistance;
        }

        // Gets total time
        public double GetTotalTime()
        {
            return TotalTime;
        }
    }
}
