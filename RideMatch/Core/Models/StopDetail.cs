using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Models
{
    public class StopDetail
    {
        // Properties for stop number, passenger ID, distances, times, etc.

        // Calculates the arrival time given a start time
        public DateTime CalculateArrivalTime(DateTime startTime);

        // Gets the formatted time string for display
        public string GetFormattedTime();
    }
}
