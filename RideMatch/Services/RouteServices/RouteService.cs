using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Services.RouteServices
{
    public class RouteService : IRouteService
    {
        // Generates optimal routes for a given date
        public Task<Solution> GenerateRoutesAsync(string date, IEnumerable<Vehicle> vehicles, IEnumerable<Passenger> passengers);

        // Gets routes for a specific date
        public Task<Solution> GetRoutesForDateAsync(string date);

        // Gets route details for a specific vehicle
        public Task<RouteDetails> GetRouteDetailsAsync(int vehicleId, string date);

        // Saves generated routes to the database
        public Task<int> SaveRoutesAsync(Solution solution, string date);

        // Gets a driver's route for a specific date
        public Task<(Vehicle Vehicle, IEnumerable<Passenger> Passengers)> GetDriverRouteAsync(int userId, string date);

        // Gets a passenger's assignment for a specific date
        public Task<(Vehicle AssignedVehicle, DateTime? PickupTime)> GetPassengerAssignmentAsync(int userId, string date);

        // Updates pickup times for a route
        public Task<bool> UpdatePickupTimesAsync(int routeId, Dictionary<int, string> passengerPickupTimes);

        // Calculates pickup times based on target arrival time
        private Task CalculatePickupTimesBasedOnTargetArrival(Solution solution, string targetTimeString, IRoutingService routingService);

        // Converts target time string to minutes
        private int GetTargetTimeInMinutes(string targetTime);
    }
}
