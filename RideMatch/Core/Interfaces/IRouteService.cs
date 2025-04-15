using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Interfaces
{
    public interface IRouteService
    {
        // Generates optimal routes for a given date
        Task<Solution> GenerateRoutesAsync(string date, IEnumerable<Vehicle> vehicles, IEnumerable<Passenger> passengers);

        // Gets routes for a specific date
        Task<Solution> GetRoutesForDateAsync(string date);

        // Gets route details for a specific vehicle
        Task<RouteDetails> GetRouteDetailsAsync(int vehicleId, string date);

        // Saves generated routes to the database
        Task<int> SaveRoutesAsync(Solution solution, string date);

        // Gets a driver's route for a specific date
        Task<(Vehicle Vehicle, IEnumerable<Passenger> Passengers)> GetDriverRouteAsync(int userId, string date);

        // Gets a passenger's assignment for a specific date
        Task<(Vehicle AssignedVehicle, DateTime? PickupTime)> GetPassengerAssignmentAsync(int userId, string date);

        // Updates pickup times for a route
        Task<bool> UpdatePickupTimesAsync(int routeId, Dictionary<int, string> passengerPickupTimes);
    }
}
