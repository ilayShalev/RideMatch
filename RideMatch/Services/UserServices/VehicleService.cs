using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideMatch.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly VehicleRepository _vehicleRepository;

        // Constructor
        public VehicleService(VehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        }

        // Gets a vehicle by ID
        public async Task<Vehicle> GetVehicleByIdAsync(int vehicleId)
        {
            if (vehicleId <= 0)
                throw new ArgumentException("Vehicle ID must be greater than zero", nameof(vehicleId));

            return await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
        }

        // Gets a vehicle by user ID
        public async Task<Vehicle> GetVehicleByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(userId));

            return await _vehicleRepository.GetVehicleByUserIdAsync(userId);
        }

        // Creates a new vehicle
        public async Task<int> CreateVehicleAsync(Vehicle vehicle)
        {
            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle));

            if (vehicle.UserId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(vehicle.UserId));

            if (vehicle.Capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero", nameof(vehicle.Capacity));

            return await _vehicleRepository.AddVehicleAsync(
                vehicle.UserId,
                vehicle.Capacity,
                vehicle.Latitude,
                vehicle.Longitude,
                vehicle.Address ?? string.Empty);
        }

        // Updates a vehicle
        public async Task<bool> UpdateVehicleAsync(Vehicle vehicle)
        {
            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle));

            if (vehicle.Id <= 0)
                throw new ArgumentException("Vehicle ID must be greater than zero", nameof(vehicle.Id));

            if (vehicle.Capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero", nameof(vehicle.Capacity));

            return await _vehicleRepository.UpdateVehicleAsync(
                vehicle.Id,
                vehicle.Capacity,
                vehicle.Latitude,
                vehicle.Longitude,
                vehicle.Address ?? string.Empty);
        }

        // Updates a vehicle's availability
        public async Task<bool> UpdateVehicleAvailabilityAsync(int vehicleId, bool isAvailable)
        {
            if (vehicleId <= 0)
                throw new ArgumentException("Vehicle ID must be greater than zero", nameof(vehicleId));

            return await _vehicleRepository.UpdateVehicleAvailabilityAsync(vehicleId, isAvailable);
        }

        // Updates a vehicle's capacity
        public async Task<bool> UpdateVehicleCapacityAsync(int vehicleId, int capacity)
        {
            if (vehicleId <= 0)
                throw new ArgumentException("Vehicle ID must be greater than zero", nameof(vehicleId));

            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero", nameof(capacity));

            return await _vehicleRepository.UpdateVehicleCapacityAsync(vehicleId, capacity);
        }

        // Updates a vehicle's location
        public async Task<bool> UpdateVehicleLocationAsync(int vehicleId, double latitude, double longitude, string address)
        {
            if (vehicleId <= 0)
                throw new ArgumentException("Vehicle ID must be greater than zero", nameof(vehicleId));

            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address cannot be null or empty", nameof(address));

            return await _vehicleRepository.UpdateVehicleLocationAsync(vehicleId, latitude, longitude, address);
        }

        // Deletes a vehicle
        public async Task<bool> DeleteVehicleAsync(int vehicleId)
        {
            if (vehicleId <= 0)
                throw new ArgumentException("Vehicle ID must be greater than zero", nameof(vehicleId));

            return await _vehicleRepository.DeleteVehicleAsync(vehicleId);
        }

        // Gets all vehicles
        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _vehicleRepository.GetAllVehiclesAsync();
        }

        // Gets all available vehicles
        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _vehicleRepository.GetAvailableVehiclesAsync();
        }
    }
}