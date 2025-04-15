using System;

namespace RideMatch.Core.Models
{
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public string TargetTime { get; set; }

        // Validates destination data
        public bool Validate()
        {
            // Name should not be empty
            if (string.IsNullOrEmpty(Name))
                return false;

            // Address should not be empty
            if (string.IsNullOrEmpty(Address))
                return false;

            // Target time should be in format HH:MM
            if (!string.IsNullOrEmpty(TargetTime))
            {
                string[] parts = TargetTime.Split(':');
                if (parts.Length != 2)
                    return false;

                if (!int.TryParse(parts[0], out int hours) || hours < 0 || hours > 23)
                    return false;

                if (!int.TryParse(parts[1], out int minutes) || minutes < 0 || minutes > 59)
                    return false;
            }

            return true;
        }
    }
}