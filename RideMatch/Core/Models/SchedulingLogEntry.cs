using System;
using System.Drawing;

namespace RideMatch.Core.Models
{
    public class SchedulingLogEntry
    {
        public int Id { get; set; }
        public DateTime RunTime { get; set; }
        public string Status { get; set; }
        public int RoutesGenerated { get; set; }
        public int PassengersAssigned { get; set; }
        public string ErrorMessage { get; set; }

        // Formatted date/time for display
        public string FormattedDateTime()
        {
            return RunTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // Status with color information for UI
        public (string Status, Color Color) GetStatusWithColor()
        {
            switch (Status?.ToLower())
            {
                case "success":
                    return (Status, Color.Green);
                case "failed":
                    return (Status, Color.Red);
                case "warning":
                    return (Status, Color.Orange);
                default:
                    return (Status ?? "Unknown", Color.Gray);
            }
        }
    }
}