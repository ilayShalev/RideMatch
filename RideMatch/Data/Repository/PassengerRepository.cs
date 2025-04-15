using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    public class PassengerRepository
    {
        // Adds a new passenger
        public Task<int> AddPassengerAsync(int userId, string name, double latitude, double longitude, string address);

        // Updates a passenger
        public Task<bool> UpdatePassengerAsync(int passengerId, string name, double latitude, double longitude, string address);

        // Updates a passenger's availability
        public Task<bool> UpdatePassengerAvailabilityAsync(int passengerId, bool isAvailable);

        // Gets available passengers
        public Task<List<Passenger>> GetAvailablePassengersAsync();

        // Gets all passengers
        public Task<List<Passenger>> GetAllPassengersAsync();

        // Gets a passenger by user ID
        public Task<Passenger> GetPassengerByUserIdAsync(int userId);

        // Gets a passenger by ID
        public Task<Passenger> GetPassengerByIdAsync(int passengerId);

        // Deletes a passenger
        public Task<bool> DeletePassengerAsync(int passengerId);
    }
}
