using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.Repository;
using RideMatch.Utilities.Geo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RideMatch.Services.RouteServices
{
    public class RoutingService : IRoutingService
    {
        private readonly IMapService _mapService;
        private readonly SettingsRepository _settingsRepository;

        // Constructor
        public RoutingService(IMapService mapService, SettingsRepository settingsRepository)
        {
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        }

        // Displays passengers and vehicles on the map
        public void DisplayDataOnMap(IMapControl mapControl, List<Passenger> passengers, List<Vehicle> vehicles)
        {
            if (mapControl == null)
                throw new ArgumentNullException(nameof(mapControl));

            // Clear previous overlays
            mapControl.ClearOverlays();

            // Display passengers
            if (passengers != null)
            {
                foreach (var passenger in passengers)
                {
                    if (passenger.IsAvailable)
                    {
                        mapControl.AddMarker(
                            passenger.Latitude,
                            passenger.Longitude,
                            MarkerType.Passenger,
                            $"Passenger: {passenger.Name}");
                    }
                }
            }

            // Display vehicles
            if (vehicles != null)
            {
                foreach (var vehicle in vehicles)
                {
                    if (vehicle.IsAvailable)
                    {
                        mapControl.AddMarker(
                            vehicle.Latitude,
                            vehicle.Longitude,
                            MarkerType.Vehicle,
                            $"Vehicle: {vehicle.DriverName} (Capacity: {vehicle.Capacity})");
                    }
                }
            }

            // Refresh the map
            mapControl.Refresh();
        }

        // Displays solution routes on the map
        public void DisplaySolutionOnMap(IMapControl mapControl, Solution solution)
        {
            if (mapControl == null)
                throw new ArgumentNullException(nameof(mapControl));

            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            // Clear previous overlays
            mapControl.ClearOverlays();

            // Get all vehicles with passengers
            var vehiclesWithPassengers = solution.Vehicles
                .Where(v => v.Passengers.Count > 0)
                .ToList();

            int routeIndex = 0;

            // For each vehicle with passengers
            foreach (var vehicle in vehiclesWithPassengers)
            {
                // Add vehicle marker
                mapControl.AddMarker(
                    vehicle.Latitude,
                    vehicle.Longitude,
                    MarkerType.Vehicle,
                    $"Vehicle: {vehicle.DriverName} (Capacity: {vehicle.Capacity})");

                // Add passenger markers
                foreach (var passenger in vehicle.Passengers)
                {
                    mapControl.AddMarker(
                        passenger.Latitude,
                        passenger.Longitude,
                        MarkerType.Passenger,
                        $"Passenger: {passenger.Name} " +
                        (passenger.PickupTime.HasValue ? $"(Pickup: {passenger.PickupTime.Value.ToString("HH:mm")})" : ""));
                }

                // Get route color
                Color routeColor = _mapService.GetRouteColor(routeIndex++);

                // Add simple straight-line routes between points
                List<PointLatLng> routePoints = new List<PointLatLng>();

                // Start from vehicle location
                routePoints.Add(new PointLatLng { Latitude = vehicle.Latitude, Longitude = vehicle.Longitude });

                // Add passenger locations
                foreach (var passenger in vehicle.Passengers)
                {
                    routePoints.Add(new PointLatLng { Latitude = passenger.Latitude, Longitude = passenger.Longitude });
                }

                // Draw the route
                if (routePoints.Count > 1)
                {
                    mapControl.AddRoute(routePoints, $"Vehicle {vehicle.Id} Route", routeColor);
                }
            }

            // Refresh the map
            mapControl.Refresh();
        }

        // Gets detailed route information using Google Maps Directions API
        public async Task GetGoogleRoutesAsync(IMapControl mapControl, Solution solution)
        {
            if (mapControl == null)
                throw new ArgumentNullException(nameof(mapControl));

            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            // Clear previous overlays
            mapControl.ClearOverlays();

            // Get destination information
            var destinationInfo = await _settingsRepository.GetDestinationAsync();
            double destinationLat = destinationInfo.Latitude;
            double destinationLng = destinationInfo.Longitude;

            // Add destination marker
            mapControl.AddMarker(
                destinationLat,
                destinationLng,
                MarkerType.Destination,
                $"Destination: {destinationInfo.Name}");

            // Get all vehicles with passengers
            var vehiclesWithPassengers = solution.Vehicles
                .Where(v => v.Passengers.Count > 0)
                .ToList();

            int routeIndex = 0;

            // For each vehicle with passengers
            foreach (var vehicle in vehiclesWithPassengers)
            {
                // Add vehicle marker
                mapControl.AddMarker(
                    vehicle.Latitude,
                    vehicle.Longitude,
                    MarkerType.Vehicle,
                    $"Vehicle: {vehicle.DriverName} (Capacity: {vehicle.Capacity})");

                // Add passenger markers
                foreach (var passenger in vehicle.Passengers)
                {
                    mapControl.AddMarker(
                        passenger.Latitude,
                        passenger.Longitude,
                        MarkerType.Passenger,
                        $"Passenger: {passenger.Name} " +
                        (passenger.PickupTime.HasValue ? $"(Pickup: {passenger.PickupTime.Value.ToString("HH:mm")})" : ""));
                }

                // Get route color
                Color routeColor = _mapService.GetRouteColor(routeIndex++);

                // Build list of waypoints (vehicle -> passengers -> destination)
                List<PointLatLng> waypoints = new List<PointLatLng>
                {
                    new PointLatLng { Latitude = vehicle.Latitude, Longitude = vehicle.Longitude }
                };

                // Add passenger waypoints
                foreach (var passenger in vehicle.Passengers)
                {
                    waypoints.Add(new PointLatLng { Latitude = passenger.Latitude, Longitude = passenger.Longitude });
                }

                // Add destination waypoint
                waypoints.Add(new PointLatLng { Latitude = destinationLat, Longitude = destinationLng });

                try
                {
                    // Get directions from Google API
                    var routePoints = await _mapService.GetDirectionsAsync(waypoints);

                    // Draw the route
                    if (routePoints.Count > 1)
                    {
                        mapControl.AddRoute(routePoints, $"Vehicle {vehicle.Id} Route", routeColor);
                    }
                }
                catch (Exception ex)
                {
                    // Log error
                    Console.WriteLine($"Error getting Google routes: {ex.Message}");

                    // Fall back to straight lines
                    if (waypoints.Count > 1)
                    {
                        mapControl.AddRoute(waypoints, $"Vehicle {vehicle.Id} Route", routeColor);
                    }
                }
            }

            // Refresh the map
            mapControl.Refresh();
        }

        // Calculates estimated route details for a solution without using Google API
        public void CalculateEstimatedRouteDetails(Solution solution)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            // Get destination from settings
            var destinationTask = _settingsRepository.GetDestinationAsync();
            destinationTask.Wait();
            var destinationInfo = destinationTask.Result;

            double destinationLat = destinationInfo.Latitude;
            double destinationLng = destinationInfo.Longitude;

            // For each vehicle with passengers
            foreach (var vehicle in solution.Vehicles.Where(v => v.Passengers.Count > 0))
            {
                // Starting point is vehicle location
                double currentLat = vehicle.Latitude;
                double currentLng = vehicle.Longitude;

                double totalDistance = 0;
                double totalTime = 0;

                // Calculate distance and time for each passenger pickup
                foreach (var passenger in vehicle.Passengers)
                {
                    // Calculate distance from current location to passenger
                    double distance = DistanceCalculator.CalculateDistance(
                        currentLat, currentLng, passenger.Latitude, passenger.Longitude);

                    // Estimate time (assume 30 km/h average speed in urban areas)
                    double time = (distance / 30) * 60; // minutes

                    totalDistance += distance;
                    totalTime += time;

                    // Update current location
                    currentLat = passenger.Latitude;
                    currentLng = passenger.Longitude;
                }

                // Add distance and time from last passenger to destination
                double finalDistance = DistanceCalculator.CalculateDistance(
                    currentLat, currentLng, destinationLat, destinationLng);

                double finalTime = (finalDistance / 30) * 60; // minutes

                totalDistance += finalDistance;
                totalTime += finalTime;

                // Set vehicle route details
                vehicle.TotalDistance = totalDistance;
                vehicle.TotalTime = totalTime;
            }
        }

        // Validates the solution for constraints
        public string ValidateSolution(Solution solution, List<Passenger> allPassengers)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            if (allPassengers == null)
                throw new ArgumentNullException(nameof(allPassengers));

            List<string> validationErrors = new List<string>();

            // Check if all available passengers are assigned
            var availablePassengers = allPassengers.Where(p => p.IsAvailable).ToList();
            var assignedPassengerIds = new HashSet<int>();

            foreach (var vehicle in solution.Vehicles)
            {
                foreach (var passenger in vehicle.Passengers)
                {
                    assignedPassengerIds.Add(passenger.Id);
                }
            }

            var unassignedPassengers = availablePassengers
                .Where(p => !assignedPassengerIds.Contains(p.Id))
                .ToList();

            if (unassignedPassengers.Count > 0)
            {
                validationErrors.Add($"Warning: {unassignedPassengers.Count} available passengers are not assigned.");
            }

            // Check vehicle capacity constraints
            foreach (var vehicle in solution.Vehicles)
            {
                if (vehicle.Passengers.Count > vehicle.Capacity)
                {
                    validationErrors.Add($"Error: Vehicle {vehicle.Id} has {vehicle.Passengers.Count} passengers but capacity is {vehicle.Capacity}.");
                }
            }

            // Check for duplicate passengers
            var duplicateCheck = new Dictionary<int, List<int>>();

            foreach (var vehicle in solution.Vehicles)
            {
                foreach (var passenger in vehicle.Passengers)
                {
                    if (!duplicateCheck.ContainsKey(passenger.Id))
                    {
                        duplicateCheck[passenger.Id] = new List<int>();
                    }

                    duplicateCheck[passenger.Id].Add(vehicle.Id);
                }
            }

            foreach (var entry in duplicateCheck)
            {
                if (entry.Value.Count > 1)
                {
                    validationErrors.Add($"Error: Passenger {entry.Key} is assigned to multiple vehicles: {string.Join(", ", entry.Value)}.");
                }
            }

            // Return validation result
            if (validationErrors.Count == 0)
            {
                return "Solution is valid.";
            }
            else
            {
                return string.Join("\n", validationErrors);
            }
        }
    }
}