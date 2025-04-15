using System;

namespace RideMatch.Core.Models
{
    public class StopDetail
    {
        public int StopNumber { get; set; }
        public int PassengerId { get; set; }
        public string PassengerName { get; set; }
        public double DistanceFromPrevious { get; set; }
        public double TimeFromPrevious { get; set; }
        public double CumulativeDistance { get; set; }
        public double CumulativeTime { get; set; }
        public DateTime? ArrivalTime { get; set; }

        // Calculates the arrival time given a start time
        public DateTime CalculateArrivalTime(DateTime startTime)
        {
            return startTime.AddMinutes(CumulativeTime);
        }

        // Gets the formatted time string for display
        public string GetFormattedTime()
        {
            return ArrivalTime?.ToString("HH:mm") ?? "--:--";
        }
    }
}