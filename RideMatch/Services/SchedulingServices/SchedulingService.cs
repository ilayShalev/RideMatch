using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.Repository;
using RideMatch.Services.RouteServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RideMatch.Services.SchedulingServices
{
    public class SchedulingService : ISchedulerService
    {
        private readonly SettingsRepository _settingsRepository;
        private readonly IRouteService _routeService;
        private readonly IVehicleService _vehicleService;
        private readonly IPassengerService _passengerService;
        private readonly IRoutingService _routingService;
        private readonly string _logFilePath;

        // Constructor
        public SchedulingService(
            SettingsRepository settingsRepository,
            IRouteService routeService,
            IVehicleService vehicleService,
        IPassengerService passengerService,
            IRoutingService routingService)
        {
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _passengerService = passengerService ?? throw new ArgumentNullException(nameof(passengerService));
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));

            // Set log file path
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RideMatch");

            // Create directory if it doesn't exist
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _logFilePath = Path.Combine(appDataPath, "scheduler.log");
        }

        // Gets scheduling settings
        public async Task<SchedulingSetting> GetSchedulingSettingsAsync()
        {
            var settings = await _settingsRepository.GetSchedulingSettingsAsync();

            return new SchedulingSetting
            {
                IsEnabled = settings.IsEnabled,
                ScheduledTime = settings.ScheduledTime
            };
        }

        // Saves scheduling settings
        public async Task<bool> SaveSchedulingSettingsAsync(bool isEnabled, DateTime scheduledTime)
        {
            try
            {
                await _settingsRepository.SaveSchedulingSettingsAsync(isEnabled, scheduledTime);
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error saving scheduling settings: {ex.Message}");
                return false;
            }
        }

        // Runs the scheduling algorithm
        public async Task<bool> RunSchedulerAsync()
        {
            DateTime runTime = DateTime.Now;
            string status = "Failed";
            int routesGenerated = 0;
            int passengersAssigned = 0;
            string errorMessage = null;

            try
            {
                Log("Starting scheduling run...");

                // Get available vehicles
                var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
                if (vehicles == null || !vehicles.Any())
                {
                    errorMessage = "No available vehicles found.";
                    Log(errorMessage);
                    await LogSchedulingRunAsync(runTime, "Failed", 0, 0, errorMessage);
                    return false;
                }

                // Get available passengers
                var passengers = await _passengerService.GetAvailablePassengersAsync();
                if (passengers == null || !passengers.Any())
                {
                    errorMessage = "No available passengers found.";
                    Log(errorMessage);
                    await LogSchedulingRunAsync(runTime, "Failed", 0, 0, errorMessage);
                    return false;
                }

                // Get today's date as string (YYYY-MM-DD)
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // Generate routes
                Log("Generating routes...");
                var solution = await _routeService.GenerateRoutesAsync(today, vehicles, passengers);

                // Calculate estimated route details
                _routingService.CalculateEstimatedRouteDetails(solution);

                // Validate solution
                string validationResult = _routingService.ValidateSolution(solution, passengers.ToList());
                Log($"Validation result: {validationResult}");

                // Save routes
                int solutionId = await _routeService.SaveRoutesAsync(solution, today);

                // Calculate metrics
                routesGenerated = solution.GetUsedVehicleCount();
                passengersAssigned = solution.GetAssignedPassengerCount();

                status = "Success";
                Log($"Scheduling completed successfully. Routes generated: {routesGenerated}, Passengers assigned: {passengersAssigned}");

                // Log the run
                await LogSchedulingRunAsync(runTime, status, routesGenerated, passengersAssigned);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log($"Error during scheduling: {errorMessage}");

                // Log the failed run
                await LogSchedulingRunAsync(runTime, "Failed", 0, 0, errorMessage);

                return false;
            }
        }

        // Gets the scheduling log
        public async Task<IEnumerable<SchedulingLogEntry>> GetSchedulingLogAsync()
        {
            var logEntries = await _settingsRepository.GetSchedulingLogAsync();

            return logEntries.Select(entry => new SchedulingLogEntry
            {
                RunTime = entry.RunTime,
                Status = entry.Status,
                RoutesGenerated = entry.RoutesGenerated,
                PassengersAssigned = entry.PassengersAssigned
            });
        }

        // Logs a scheduling run
        public async Task<bool> LogSchedulingRunAsync(DateTime runTime, string status, int routesGenerated, int passengersAssigned, string message = null)
        {
            try
            {
                await _settingsRepository.LogSchedulingRunAsync(
                    runTime,
                    status,
                    routesGenerated,
                    passengersAssigned,
                    message);

                return true;
            }
            catch (Exception ex)
            {
                Log($"Error logging scheduling run: {ex.Message}");
                return false;
            }
        }

        // Calculates pickup times based on target arrival time
        private async Task CalculatePickupTimesBasedOnTargetArrival(Solution solution, string targetTimeString, IRoutingService routingService)
        {
            if (solution == null || string.IsNullOrEmpty(targetTimeString))
                return;

            // Get destination information
            var destinationInfo = await _settingsRepository.GetDestinationAsync();

            // Convert target time to DateTime
            if (!DateTime.TryParse($"{DateTime.Today.ToString("yyyy-MM-dd")} {targetTimeString}", out DateTime targetTime))
                return;

            // For each vehicle with passengers
            foreach (var vehicle in solution.Vehicles.Where(v => v.Passengers.Count > 0))
            {
                // Remaining time to destination
                double remainingTime = vehicle.TotalTime; // minutes

                // For each passenger
                for (int i = 0; i < vehicle.Passengers.Count; i++)
                {
                    // Calculate pickup time (target time - remaining time)
                    DateTime pickupTime = targetTime.AddMinutes(-remainingTime);
                    vehicle.Passengers[i].PickupTime = pickupTime;

                    // Update remaining time (subtract time to next passenger)
                    if (i < vehicle.Passengers.Count - 1)
                    {
                        // Estimate time between current and next passenger
                        double distanceBetweenPassengers = CalculateDistance(
                            vehicle.Passengers[i].Latitude,
                            vehicle.Passengers[i].Longitude,
                            vehicle.Passengers[i + 1].Latitude,
                            vehicle.Passengers[i + 1].Longitude);

                        double timeBetweenPassengers = (distanceBetweenPassengers / 30) * 60; // minutes
                        remainingTime -= timeBetweenPassengers;
                    }
                }
            }
        }

        // Helper method to calculate distance
        private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double earthRadius = 6371; // km

            double dLat = ToRadians(lat2 - lat1);
            double dLng = ToRadians(lng2 - lng1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                      Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadius * c;
        }

        // Helper method to convert degrees to radians
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        // Logs a message to the service log file
        private void Log(string message)
        {
            try
            {
                // Append to log file
                using (StreamWriter writer = File.AppendText(_logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}