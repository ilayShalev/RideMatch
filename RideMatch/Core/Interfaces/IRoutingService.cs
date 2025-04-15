using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Interfaces
{
    public interface IRoutingService
    {
        // Displays passengers and vehicles on the map
        void DisplayDataOnMap(IMapControl mapControl, List<Passenger> passengers, List<Vehicle> vehicles);

        // Displays solution routes on the map
        void DisplaySolutionOnMap(IMapControl mapControl, Solution solution);

        // Gets detailed route information using Google Maps Directions API
        Task GetGoogleRoutesAsync(IMapControl mapControl, Solution solution);

        // Calculates estimated route details for a solution without using Google API
        void CalculateEstimatedRouteDetails(Solution solution);

        // Validates the solution for constraints
        string ValidateSolution(Solution solution, List<Passenger> allPassengers);
    }
}
