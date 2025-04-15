using System;

namespace RideMatch.Core.Models
{
    public class SchedulingSetting
    {
        public bool IsEnabled { get; set; }
        public DateTime ScheduledTime { get; set; }

        // Validates scheduling settings
        public bool Validate()
        {
            // If not enabled, no other validation needed
            if (!IsEnabled)
                return true;

            // Scheduled time should be in the future
            if (ScheduledTime.TimeOfDay < DateTime.Now.TimeOfDay)
                return false;

            return true;
        }
    }
}