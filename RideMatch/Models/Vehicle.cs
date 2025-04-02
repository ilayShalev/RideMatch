using System;

namespace RideMatch.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string LicensePlate { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime LastUpdate { get; set; }

        public Vehicle()
        {
            IsAvailable = true;
            LastUpdate = DateTime.Now;
        }
    }
} 