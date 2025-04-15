using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    public class VehicleRepository
    {
        // Adds a new vehicle
        public Task<int> AddVehicleAsync(int userId, int capacity, double startLatitude, double startLongitude, string startAddress);

        // Updates a vehicle
        public Task<bool> UpdateVehicleAsync(int vehicleId, int capacity, double startLatitude, double startLongitude, string startAddress);

        // Updates a vehicle's availability
        public Task<bool> UpdateVehicleAvailabilityAsync(int vehicleId, bool isAvailable);

        // Gets all vehicles
        public Task<List<Vehicle>> GetAllVehiclesAsync();

        // Gets available vehicles
        public Task<List<Vehicle>> GetAvailableVehiclesAsync();

        // Gets a vehicle by user ID
        public Task<Vehicle> GetVehicleByUserIdAsync(int userId);

        // Gets a vehicle by ID
        public Task<Vehicle> GetVehicleByIdAsync(int vehicleId);

        // Saves or updates a driver's vehicle
        public Task<int> SaveDriverVehicleAsync(int userId, int capacity, double startLatitude, double startLongitude, string startAddress);
            ;
        // Updates a vehicle's capacity
        public Task<bool> UpdateVehicleCapacityAsync(int userId, int capacity);

        // Updates a vehicle's location
        public Task<bool> UpdateVehicleLocationAsync(int userId, double latitude, double longitude, string address);

        // Deletes a vehicle
        public Task<bool> DeleteVehicleAsync(int vehicleId);
    }
}
