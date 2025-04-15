using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.Repository;
using RideMatch.Services.MapServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideMatch.Services.RouteServices
{
    public class RouteService : IRouteService
    {
        private readonly RouteRepository _routeRepository;
        private readonly IRoutingService _routingService;
        private readonly SettingsRepository _settingsRepository;

        // Constructor
        public RouteService(RouteRepository routeRepository, IRoutingService routingService, SettingsRepository settingsRepository)
        {
            _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        }

        // Generates optimal routes for a given date
        public async Task<Solution> GenerateRoutesAsync(string date, IEnumerable<Vehicle> vehicles, IEnumerable<Passenger> passengers)
        {
            if (string.IsNullOrEmpty(date))
                throw new ArgumentException("Date cannot be null or empty", nameof(date));

            if (vehicles == null || !vehicles.Any())
                throw new ArgumentException("Vehicles cannot be null or empty", nameof(vehicles));

            if (passengers == null || !passengers.Any())
                throw new ArgumentException("Passengers cannot be null or empty", nameof(passengers));

            // Get destination information
            var destinationInfo = await _settingsRepository.GetDestinationAsync();
            double destinationLat = destinationInfo.Latitude;
            double destinationLng = destinationInfo.Longitude;
            string targetTime = destinationInfo.TargetTime;

            // Create a new algorithm instance
            var algorithm = new RideSharingAlgorithm(
                vehicles.ToList(),
                passengers.ToList(),
                destinationLat,
                destinationLng);

            // Run the algorithm for 100 generations
            Solution solution = algorithm.Solve(100);

            // Calculate pickup times based on target arrival time
            await CalculatePickupTimesBasedOnTargetArrival(solution, targetTime, _routingService);

            return solution;
        }

        // Gets routes for a specific date
        public async Task<Solution> GetRoutesForDateAsync(string date)
        {
            if (string.IsNullOrEmpty(date))
                throw new ArgumentException("Date cannot be null or empty", nameof(date));

            return await _routeRepository.GetSolutionForDateAsync(date);
        }

        // Gets route details for a specific vehicle
        public async Task<RouteDetails> GetRouteDetailsAsync(int vehicleId, string date)
        {
            if (string.IsNullOrEmpty(date))
                throw new ArgumentException("Date cannot be null or empty", nameof(date));

            if (vehicleId <= 0)
                throw new ArgumentException("Vehicle ID must be greater than zero", nameof(vehicleId));

            // Get the solution for the date
            var solution = await GetRoutesForDateAsync(date);
            if (solution == null)
                return null;

            // Find the vehicle in the solution
            var vehicle = solution.Vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null || vehicle.Passengers.Count == 0)
                return null;

            // Create route details object
            var routeDetails = new RouteDetails
            {
                VehicleId = vehicleId,
                TotalDistance = vehicle.TotalDistance,
                TotalTime = vehicle.TotalTime,
                StopDetails = new List<StopDetail>()
            };

            // Add each passenger as a stop
            double cumulativeDistance = 0;
            double cumulativeTime = 0;

            for (int i = 0; i < vehicle.Passengers.Count; i++)
            {
                var passenger = vehicle.Passengers[i];

                // Previous location (vehicle location for first passenger)
                double prevLat = (i == 0) ? vehicle.Latitude : vehicle.Passengers[i - 1].Latitude;
                double prevLng = (i == 0) ? vehicle.Longitude : vehicle.Passengers[i - 1].Longitude;

                // Calculate distance and time from previous stop
                double distanceFromPrev = CalculateDistance(prevLat, prevLng, passenger.Latitude, passenger.Longitude);
                double timeFromPrev = CalculateTime(distanceFromPrev);

                cumulativeDistance += distanceFromPrev;
                cumulativeTime += timeFromPrev;

                // Add stop detail
                routeDetails.AddStop(
                    passenger.Id,
                    passenger.Name,
                    distanceFromPrev,
                    timeFromPrev);
            }

            // Add destination as final stop
            var destinationInfo = await _settingsRepository.GetDestinationAsync();
            double destinationLat = destinationInfo.Latitude;
            double destinationLng = destinationInfo.Longitude;

            // Calculate distance and time from last passenger to destination
            var lastPassenger = vehicle.Passengers.Last();
            double distToDestination = CalculateDistance(lastPassenger.Latitude, lastPassenger.Longitude, destinationLat, destinationLng);
            double timeToDestination = CalculateTime(distToDestination);

            // Add destination stop
            routeDetails.AddStop(
                0, // ID 0 for destination
                "Destination",
                distToDestination,
                timeToDestination);

            // Calculate cumulative values
            routeDetails.CalculateCumulatives();

            return routeDetails;
        }

        // Saves generated routes to the database
        public async Task<int> SaveRoutesAsync(Solution solution, string date)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            if (string.IsNullOrEmpty(date))
                throw new ArgumentException("Date cannot be null or empty", nameof(date));

            return await _routeRepository.SaveSolutionAsync(solution, date);
        }

        // Gets a driver's route for a specific date
        public async Task<(Vehicle Vehicle, IEnumerable<Passenger> Passengers)> GetDriverRouteAsync(int userId, string date)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(userId));

            if (string.IsNullOrEmpty(date))
                throw new ArgumentException("Date cannot be null or empty", nameof(date));

            var result = await _routeRepository.GetDriverRouteAsync(userId, date);
            return (result.Vehicle, result.Passengers);
        }

        // Gets a passenger's assignment for a specific date
        public async Task<(Vehicle AssignedVehicle, DateTime? PickupTime)> GetPassengerAssignmentAsync(int userId, string date)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(userId));

            if (string.IsNullOrEmpty(date))
                throw new ArgumentException("Date cannot be null or empty", nameof(date));

            return await _routeRepository.GetPassengerAssignmentAsync(userId, date);
        }

        // Updates pickup times for a route
        public async Task<bool> UpdatePickupTimesAsync(int routeId, Dictionary<int, string> passengerPickupTimes)
        {
            if (routeId <= 0)
                throw new ArgumentException("Route ID must be greater than zero", nameof(routeId));

            if (passengerPickupTimes == null || !passengerPickupTimes.Any())
                throw new ArgumentException("Passenger pickup times cannot be null or empty", nameof(passengerPickupTimes));

            return await _routeRepository.UpdatePickupTimesAsync(routeId, passengerPickupTimes);
        }

        // Calculates pickup times based on target arrival time
        private async Task CalculatePickupTimesBasedOnTargetArrival(Solution solution, string targetTimeString, IRoutingService routingService)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            if (string.IsNullOrEmpty(targetTimeString))
                return;

            // Get target time in minutes (e.g., "8:30" -> 510 minutes)
            int targetTimeMinutes = GetTargetTimeInMinutes(targetTimeString);

            // For each vehicle with passengers
            foreach (var vehicle in solution.Vehicles.Where(v => v.Passengers.Count > 0))
            {
                // Get detailed route information
                var routeDetails = new RouteDetails
                {
                    VehicleId = vehicle.Id,
                    TotalDistance = 0,
                    TotalTime = 0,
                    StopDetails = new List<StopDetail>()
                };

                // Start location is vehicle location
                double currentLat = vehicle.Latitude;
                double currentLng = vehicle.Longitude;

                // Add each passenger as a stop
                for (int i = 0; i < vehicle.Passengers.Count; i++)
                {
                    var passenger = vehicle.Passengers[i];

                    // Calculate distance and time from current location to passenger
                    double distance = CalculateDistance(currentLat, currentLng, passenger.Latitude, passenger.Longitude);
                    double time = CalculateTime(distance);

                    // Add stop
                    routeDetails.AddStop(passenger.Id, passenger.Name, distance, time);

                    // Update current location
                    currentLat = passenger.Latitude;
                    currentLng = passenger.Longitude;
                }

                // Get destination coordinates
                var destinationInfo = await _settingsRepository.GetDestinationAsync();
                double destinationLat = destinationInfo.Latitude;
                double destinationLng = destinationInfo.Longitude;

                // Add final segment to destination
                double finalDistance = CalculateDistance(currentLat, currentLng, destinationLat, destinationLng);
                double finalTime = CalculateTime(finalDistance);

                // Add destination stop
                routeDetails.AddStop(0, "Destination", finalDistance, finalTime);

                // Calculate cumulative distances and times
                routeDetails.CalculateCumulatives();

                // Set total distance and time for the vehicle
                vehicle.TotalDistance = routeDetails.GetTotalDistance();
                vehicle.TotalTime = routeDetails.GetTotalTime();

                // Calculate pickup times based on target arrival
                DateTime targetTime = DateTime.Today.AddMinutes(targetTimeMinutes);

                // For each passenger, calculate pickup time by subtracting travel time from target
                for (int i = 0; i < vehicle.Passengers.Count; i++)
                {
                    var stopDetail = routeDetails.StopDetails[i];
                    double timeFromStopToDestination = routeDetails.StopDetails.Last().CumulativeTime - stopDetail.CumulativeTime;

                    // Set pickup time
                    DateTime pickupTime = targetTime.AddMinutes(-timeFromStopToDestination);
                    vehicle.Passengers[i].PickupTime = pickupTime;
                }
            }
        }

        // Converts target time string to minutes
        private int GetTargetTimeInMinutes(string targetTime)
        {
            if (string.IsNullOrEmpty(targetTime))
                return 0;

            // Format is expected to be "HH:MM"
            string[] parts = targetTime.Split(':');
            if (parts.Length != 2)
                return 0;

            if (!int.TryParse(parts[0], out int hours) || !int.TryParse(parts[1], out int minutes))
                return 0;

            return hours * 60 + minutes;
        }

        // Helper method to calculate distance between two points
        private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            // Using Haversine formula
            const double earthRadius = 6371; // km

            double dLat = ToRadians(lat2 - lat1);
            double dLng = ToRadians(lng2 - lng1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                      Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadius * c;
        }

        // Helper method to calculate travel time based on distance
        private double CalculateTime(double distance)
        {
            // Assume average speed of 30 km/h in urban areas
            // Return time in minutes
            return (distance / 30) * 60;
        }

        // Helper method to convert degrees to radians
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}