using System;

namespace RideMatch.Models
{
    public class Passenger
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime LastUpdate { get; set; }
        public int? AssignedDriverId { get; set; }
        public DateTime? PickupTime { get; set; }

        public Passenger()
        {
            IsAvailable = true;
            LastUpdate = DateTime.Now;
        }
    }
} 