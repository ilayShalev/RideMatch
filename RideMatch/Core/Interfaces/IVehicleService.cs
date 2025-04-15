using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Interfaces
{
    public interface IVehicleService
    {
        // Gets a vehicle by ID
        Task<Vehicle> GetVehicleByIdAsync(int vehicleId);

        // Gets a vehicle by user ID
        Task<Vehicle> GetVehicleByUserIdAsync(int userId);

        // Creates a new vehicle
        Task<int> CreateVehicleAsync(Vehicle vehicle);

        // Updates a vehicle
        Task<bool> UpdateVehicleAsync(Vehicle vehicle);

        // Updates a vehicle's availability
        Task<bool> UpdateVehicleAvailabilityAsync(int vehicleId, bool isAvailable);

        // Updates a vehicle's capacity
        Task<bool> UpdateVehicleCapacityAsync(int vehicleId, int capacity);

        // Updates a vehicle's location
        Task<bool> UpdateVehicleLocationAsync(int vehicleId, double latitude, double longitude, string address);

        // Deletes a vehicle
        Task<bool> DeleteVehicleAsync(int vehicleId);

        // Gets all vehicles
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();

        // Gets all available vehicles
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
    }
}
