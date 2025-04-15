using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideMatch.Services
{
    public class PassengerService : IPassengerService
    {
        private readonly PassengerRepository _passengerRepository;

        // Constructor
        public PassengerService(PassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository ?? throw new ArgumentNullException(nameof(passengerRepository));
        }

        // Gets a passenger by ID
        public async Task<Passenger> GetPassengerByIdAsync(int passengerId)
        {
            if (passengerId <= 0)
                throw new ArgumentException("Passenger ID must be greater than zero", nameof(passengerId));

            return await _passengerRepository.GetPassengerByIdAsync(passengerId);
        }

        // Gets a passenger by user ID
        public async Task<Passenger> GetPassengerByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(userId));

            return await _passengerRepository.GetPassengerByUserIdAsync(userId);
        }

        // Creates a new passenger
        public async Task<int> CreatePassengerAsync(Passenger passenger)
        {
            if (passenger == null)
                throw new ArgumentNullException(nameof(passenger));

            if (passenger.UserId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(passenger.UserId));

            if (string.IsNullOrEmpty(passenger.Name))
                throw new ArgumentException("Name cannot be null or empty", nameof(passenger.Name));

            return await _passengerRepository.AddPassengerAsync(
                passenger.UserId,
                passenger.Name,
                passenger.Latitude,
                passenger.Longitude,
                passenger.Address ?? string.Empty);
        }

        // Updates a passenger
        public async Task<bool> UpdatePassengerAsync(Passenger passenger)
        {
            if (passenger == null)
                throw new ArgumentNullException(nameof(passenger));

            if (passenger.Id <= 0)
                throw new ArgumentException("Passenger ID must be greater than zero", nameof(passenger.Id));

            if (string.IsNullOrEmpty(passenger.Name))
                throw new ArgumentException("Name cannot be null or empty", nameof(passenger.Name));

            return await _passengerRepository.UpdatePassengerAsync(
                passenger.Id,
                passenger.Name,
                passenger.Latitude,
                passenger.Longitude,
                passenger.Address ?? string.Empty);
        }

        // Updates a passenger's availability
        public async Task<bool> UpdatePassengerAvailabilityAsync(int passengerId, bool isAvailable)
        {
            if (passengerId <= 0)
                throw new ArgumentException("Passenger ID must be greater than zero", nameof(passengerId));

            return await _passengerRepository.UpdatePassengerAvailabilityAsync(passengerId, isAvailable);
        }

        // Updates a passenger's location
        public async Task<bool> UpdatePassengerLocationAsync(int passengerId, double latitude, double longitude, string address)
        {
            if (passengerId <= 0)
                throw new ArgumentException("Passenger ID must be greater than zero", nameof(passengerId));

            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address cannot be null or empty", nameof(address));

            // Get current passenger
            var passenger = await GetPassengerByIdAsync(passengerId);
            if (passenger == null)
                return false;

            // Update location
            passenger.Latitude = latitude;
            passenger.Longitude = longitude;
            passenger.Address = address;

            // Save changes
            return await UpdatePassengerAsync(passenger);
        }

        // Deletes a passenger
        public async Task<bool> DeletePassengerAsync(int passengerId)
        {
            if (passengerId <= 0)
                throw new ArgumentException("Passenger ID must be greater than zero", nameof(passengerId));

            return await _passengerRepository.DeletePassengerAsync(passengerId);
        }

        // Gets all passengers
        public async Task<IEnumerable<Passenger>> GetAllPassengersAsync()
        {
            return await _passengerRepository.GetAllPassengersAsync();
        }

        // Gets all available passengers
        public async Task<IEnumerable<Passenger>> GetAvailablePassengersAsync()
        {
            return await _passengerRepository.GetAvailablePassengersAsync();
        }
    }
}