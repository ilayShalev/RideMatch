using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Interfaces
{
    public interface IPassengerService
    {
        // Gets a passenger by ID
        Task<Passenger> GetPassengerByIdAsync(int passengerId);

        // Gets a passenger by user ID
        Task<Passenger> GetPassengerByUserIdAsync(int userId);

        // Creates a new passenger
        Task<int> CreatePassengerAsync(Passenger passenger);

        // Updates a passenger
        Task<bool> UpdatePassengerAsync(Passenger passenger);

        // Updates a passenger's availability
        Task<bool> UpdatePassengerAvailabilityAsync(int passengerId, bool isAvailable);

        // Updates a passenger's location
        Task<bool> UpdatePassengerLocationAsync(int passengerId, double latitude, double longitude, string address);

        // Deletes a passenger
        Task<bool> DeletePassengerAsync(int passengerId);

        // Gets all passengers
        Task<IEnumerable<Passenger>> GetAllPassengersAsync();

        // Gets all available passengers
        Task<IEnumerable<Passenger>> GetAvailablePassengersAsync();
    }
}
