using System.Collections.Generic;
using System.Threading.Tasks;
using RideMatch.Core.Models;

namespace RideMatch.Core.Interfaces
{
    /// <summary>
    /// Interface for routing service
    /// </summary>
    public interface IRoutingService
    {
        /// <summary>
        /// Displays passengers and vehicles on the map
        /// </summary>
        /// <param name="mapControl">Map control</param>
        /// <param name="passengers">List of passengers</param>
        /// <param name="vehicles">List of vehicles</param>
        void DisplayDataOnMap(IMapControl mapControl, List<Passenger> passengers, List<Vehicle> vehicles);

        /// <summary>
        /// Displays solution routes on the map
        /// </summary>
        /// <param name="mapControl">Map control</param>
        /// <param name="solution">Solution with routes</param>
        void DisplaySolutionOnMap(IMapControl mapControl, Solution solution);

        /// <summary>
        /// Gets detailed route information using Google Maps Directions API
        /// </summary>
        /// <param name="mapControl">Map control</param>
        /// <param name="solution">Solution with routes</param>
        /// <returns>Task that completes when routes are retrieved</returns>
        Task GetGoogleRoutesAsync(IMapControl mapControl, Solution solution);

        /// <summary>
        /// Calculates estimated route details for a solution without using Google API
        /// </summary>
        /// <param name="solution">Solution with routes</param>
        void CalculateEstimatedRouteDetails(Solution solution);

        /// <summary>
        /// Validates the solution for constraints
        /// </summary>
        /// <param name="solution">Solution to validate</param>
        /// <param name="allPassengers">All available passengers</param>
        /// <returns>Validation results as a string</returns>
        string ValidateSolution(Solution solution, List<Passenger> allPassengers);
    }
}