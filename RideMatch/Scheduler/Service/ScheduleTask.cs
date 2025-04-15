using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideMatch.Scheduler.Service
{
    /// <summary>
    /// Task that executes the scheduling algorithm
    /// </summary>
    public class ScheduleTask
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPassengerService _passengerService;
        private readonly IRouteService _routeService;
        private readonly IRoutingService _routingService;
        private readonly SettingsRepository _settingsRepository;
        private readonly string _date;

        /// <summary>
        /// Initializes a new instance of the ScheduleTask class
        /// </summary>
        public ScheduleTask(
            IVehicleService vehicleService,
            IPassengerService passengerService,
            IRouteService routeService,
            IRoutingService routingService,
            SettingsRepository settingsRepository,
            string date = null)
        {
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _passengerService = passengerService ?? throw new ArgumentNullException(nameof(passengerService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));

            // Use provided date or default to today
            _date = date ?? DateTime.Now.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Executes the scheduling algorithm
        /// </summary>
        /// <returns>True if executed successfully, false otherwise</returns>
        public async Task<bool> ExecuteAsync()
        {
            try
            {
                // Log execution start
                await LogExecutionAsync(true, 0, 0, "Execution started");

                // Get available vehicles
                var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
                if (vehicles == null || !vehicles.Any())
                {
                    await LogExecutionAsync(false, 0, 0, "No available vehicles found");
                    return false;
                }

                // Get available passengers
                var passengers = await _passengerService.GetAvailablePassengersAsync();
                if (passengers == null || !passengers.Any())
                {
                    await LogExecutionAsync(false, 0, 0, "No available passengers found");
                    return false;
                }

                // Generate routes
                var solution = await GenerateRoutesAsync(vehicles, passengers);
                if (solution == null)
                {
                    await LogExecutionAsync(false, 0, 0, "Failed to generate routes");
                    return false;
                }

                // Calculate pickup times
                var destination = await _settingsRepository.GetDestinationAsync();
                DateTime targetTime = DateTime.Today;
                if (!string.IsNullOrEmpty(destination.TargetTime))
                {
                    // Parse target time
                    if (TimeSpan.TryParse(destination.TargetTime, out TimeSpan targetTimeSpan))
                    {
                        targetTime = DateTime.Today.Add(targetTimeSpan);
                    }
                }

                // Calculate pickup times
                await CalculatePickupTimesAsync(solution, targetTime);

                // Save solution
                int routeId = await SaveSolutionAsync(solution, _date);
                if (routeId <= 0)
                {
                    await LogExecutionAsync(false, 0, 0, "Failed to save routes");
                    return false;
                }

                // Log successful execution
                int vehiclesUsed = solution.GetUsedVehicleCount();
                int passengersAssigned = solution.GetAssignedPassengerCount();
                await LogExecutionAsync(true, vehiclesUsed, passengersAssigned);

                return true;
            }
            catch (Exception ex)
            {
                // Log execution error
                await LogExecutionAsync(false, 0, 0, $"Error during execution: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates optimal routes
        /// </summary>
        /// <param name="vehicles">The available vehicles</param>
        /// <param name="passengers">The available passengers</param>
        /// <returns>The optimized solution</returns>
        private async Task<Solution> GenerateRoutesAsync(IEnumerable<Vehicle> vehicles, IEnumerable<Passenger> passengers)
        {
            try
            {
                // Use the route service to generate optimal routes
                var solution = await _routeService.GenerateRoutesAsync(_date, vehicles, passengers);

                // Calculate estimated route details
                _routingService.CalculateEstimatedRouteDetails(solution);

                // Validate the solution
                string validationResult = _routingService.ValidateSolution(solution, passengers.ToList());

                // Check if there are critical validation errors (errors that would prevent the routes from being executed)
                if (validationResult.Contains("Error:"))
                {
                    throw new Exception($"Route validation failed: {validationResult}");
                }

                return solution;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating routes: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Calculates pickup times based on target arrival
        /// </summary>
        /// <param name="solution">The route solution</param>
        /// <param name="targetTime">The target arrival time</param>
        private async Task CalculatePickupTimesAsync(Solution solution, DateTime targetTime)
        {
            try
            {
                // Get destination information
                var destinationInfo = await _settingsRepository.GetDestinationAsync();

                foreach (var vehicle in solution.Vehicles.Where(v => v.Passengers.Count > 0))
                {
                    // Calculate last leg time (last passenger to destination)
                    double lastLegDistance = 0;
                    if (vehicle.Passengers.Any())
                    {
                        var lastPassenger = vehicle.Passengers.Last();
                        lastLegDistance = CalculateDistance(
                            lastPassenger.Latitude, lastPassenger.Longitude,
                            destinationInfo.Latitude, destinationInfo.Longitude);
                    }

                    double lastLegTime = CalculateTime(lastLegDistance);

                    // Calculate arrival at last passenger
                    DateTime lastPassengerArrival = targetTime.AddMinutes(-lastLegTime);

                    // Work backwards to calculate pickup times
                    if (vehicle.Passengers.Count > 0)
                    {
                        for (int i = vehicle.Passengers.Count - 1; i >= 0; i--)
                        {
                            // Set pickup time for current passenger
                            vehicle.Passengers[i].PickupTime = lastPassengerArrival;

                            // If not the first passenger, calculate time to previous passenger
                            if (i > 0)
                            {
                                double distance = CalculateDistance(
                                    vehicle.Passengers[i].Latitude, vehicle.Passengers[i].Longitude,
                                    vehicle.Passengers[i - 1].Latitude, vehicle.Passengers[i - 1].Longitude);

                                double time = CalculateTime(distance);

                                // Update last passenger arrival time for next iteration
                                lastPassengerArrival = lastPassengerArrival.AddMinutes(-time);
                            }
                        }

                        // Calculate time from vehicle to first passenger
                        if (vehicle.Passengers.Count > 0)
                        {
                            double distance = CalculateDistance(
                                vehicle.Latitude, vehicle.Longitude,
                                vehicle.Passengers[0].Latitude, vehicle.Passengers[0].Longitude);

                            double time = CalculateTime(distance);

                            // Set vehicle departure time
                            DateTime departureTime = vehicle.Passengers[0].PickupTime.Value.AddMinutes(-time);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating pickup times: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves the generated solution
        /// </summary>
        /// <param name="solution">The solution to save</param>
        /// <param name="date">The date for the solution</param>
        /// <returns>The route ID</returns>
        private async Task<int> SaveSolutionAsync(Solution solution, string date)
        {
            try
            {
                // Use the route service to save the solution
                return await _routeService.SaveRoutesAsync(solution, date);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving solution: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Logs the execution results
        /// </summary>
        /// <param name="success">Whether the execution was successful</param>
        /// <param name="vehiclesUsed">The number of vehicles used</param>
        /// <param name="passengersAssigned">The number of passengers assigned</param>
        /// <param name="message">Optional message</param>
        private async Task LogExecutionAsync(bool success, int vehiclesUsed, int passengersAssigned, string message = null)
        {
            try
            {
                // Log to scheduler log
                await _settingsRepository.LogSchedulingRunAsync(
                    DateTime.Now,
                    success ? "Success" : "Failed",
                    vehiclesUsed,
                    passengersAssigned,
                    message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging execution: {ex.Message}");
            }
        }

        #region Helper Methods

        /// <summary>
        /// Calculates the distance between two points
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Earth radius in kilometers
            const double R = 6371;

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        /// <summary>
        /// Calculates the travel time based on distance
        /// </summary>
        private double CalculateTime(double distance)
        {
            // Assuming average speed of 30 km/h in urban areas
            return (distance / 30) * 60; // Convert to minutes
        }

        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        #endregion
    }
}