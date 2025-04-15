using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    public class RouteRepository
    {
        // Saves a solution to the database
        public Task<int> SaveSolutionAsync(Solution solution, string date);

        // Gets driver's route and assigned passengers for a specific date
        public Task<(Vehicle Vehicle, List<Passenger> Passengers, DateTime? PickupTime)> GetDriverRouteAsync(int userId, string date);

        // Gets the solution for a specific date
        public Task<Solution> GetSolutionForDateAsync(string date);

        // Gets passenger assignment and vehicle for a specific date
        public Task<(Vehicle AssignedVehicle, DateTime? PickupTime)> GetPassengerAssignmentAsync(int userId, string date);

        // Updates the estimated pickup times for a route
        public Task<bool> UpdatePickupTimesAsync(int routeDetailId, Dictionary<int, string> passengerPickupTimes);

        // Resets all availability flags for a new day
        public Task ResetAvailabilityAsync();

        // Gets route history
        public Task<List<(int RouteId, DateTime GeneratedTime, int VehicleCount, int PassengerCount)>> GetRouteHistoryAsync();
    }
}
