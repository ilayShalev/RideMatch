using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Controls;
using RideMatch.Utilities.Geo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RideMatch.Services.MapServices
{
    public class MapService : IMapService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly GeocodingService _geocodingService;
        private readonly string _googleMapsApiKey;
        private bool _disposed;

        // Color palette for routes
        private readonly Color[] _routeColors = new Color[]
        {
            Color.Blue,
            Color.Red,
            Color.Green,
            Color.Purple,
            Color.Orange,
            Color.Brown,
            Color.Magenta,
            Color.DarkCyan,
            Color.DarkGreen,
            Color.DarkOrange
        };

        // Constructor
        public MapService(string googleMapsApiKey)
        {
            _googleMapsApiKey = googleMapsApiKey ?? throw new ArgumentNullException(nameof(googleMapsApiKey));
            _httpClient = new HttpClient();
            _geocodingService = new GeocodingService(googleMapsApiKey);
        }

        // Initializes the map with default position
        public void InitializeMap(IMapControl mapControl, double latitude = 32.0741, double longitude = 34.7922)
        {
            if (mapControl == null)
                throw new ArgumentNullException(nameof(mapControl));

            // Set initial position and zoom level
            mapControl.SetPosition(latitude, longitude, 12);

            // Default to first provider (e.g., Google Maps)
            mapControl.ChangeProvider(0);
        }

        // Changes the map provider type
        public void ChangeMapProvider(IMapControl mapControl, int providerType)
        {
            if (mapControl == null)
                throw new ArgumentNullException(nameof(mapControl));

            mapControl.ChangeProvider(providerType);
        }

        // Gets directions between a list of points
        public async Task<List<PointLatLng>> GetDirectionsAsync(List<PointLatLng> waypoints)
        {
            if (waypoints == null || waypoints.Count < 2)
                throw new ArgumentException("At least two waypoints are required", nameof(waypoints));

            try
            {
                // Build Google Directions API URL
                var origin = waypoints[0];
                var destination = waypoints[waypoints.Count - 1];

                string waypointsStr = string.Empty;
                if (waypoints.Count > 2)
                {
                    var intermediatePoints = new List<string>();
                    for (int i = 1; i < waypoints.Count - 1; i++)
                    {
                        intermediatePoints.Add($"{waypoints[i].Latitude},{waypoints[i].Longitude}");
                    }
                    waypointsStr = "&waypoints=" + string.Join("|", intermediatePoints);
                }

                string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin.Latitude},{origin.Longitude}" +
                             $"&destination={destination.Latitude},{destination.Longitude}" +
                             $"{waypointsStr}&key={_googleMapsApiKey}";

                // Send request
                var response = await _httpClient.GetStringAsync(url);

                // Parse response
                var document = JsonDocument.Parse(response);
                var root = document.RootElement;

                if (root.GetProperty("status").GetString() != "OK")
                {
                    throw new Exception($"Google Directions API error: {root.GetProperty("status").GetString()}");
                }

                // Extract route points
                var route = root.GetProperty("routes")[0];
                var legs = route.GetProperty("legs");
                var points = new List<PointLatLng>();

                for (int i = 0; i < legs.GetArrayLength(); i++)
                {
                    var steps = legs[i].GetProperty("steps");

                    for (int j = 0; j < steps.GetArrayLength(); j++)
                    {
                        var step = steps[j];
                        var polyline = step.GetProperty("polyline").GetProperty("points").GetString();

                        // Decode Google's polyline format
                        var decodedPoints = PolylineEncoder.Decode(polyline);
                        points.AddRange(decodedPoints);
                    }
                }

                return points;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error getting directions: {ex.Message}");

                // Fall back to straight lines between points
                return waypoints;
            }
        }

        // Gets detailed route information using Google Maps API
        public async Task<RouteDetails> GetRouteDetailsAsync(Vehicle vehicle, double destinationLat, double destinationLng)
        {
            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle));

            try
            {
                // Create route details object
                var routeDetails = new RouteDetails
                {
                    VehicleId = vehicle.Id,
                    TotalDistance = 0,
                    TotalTime = 0,
                    StopDetails = new List<StopDetail>()
                };

                // If no passengers, return empty route details
                if (vehicle.Passengers.Count == 0)
                    return routeDetails;

                // Build list of waypoints (vehicle -> passengers -> destination)
                var waypoints = new List<PointLatLng>
                {
                    new PointLatLng { Latitude = vehicle.Latitude, Longitude = vehicle.Longitude }
                };

                // Add passengers as waypoints
                foreach (var passenger in vehicle.Passengers)
                {
                    waypoints.Add(new PointLatLng { Latitude = passenger.Latitude, Longitude = passenger.Longitude });
                }

                // Add destination
                waypoints.Add(new PointLatLng { Latitude = destinationLat, Longitude = destinationLng });

                // Build Google Directions API URL
                string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={vehicle.Latitude},{vehicle.Longitude}" +
                             $"&destination={destinationLat},{destinationLng}";

                // Add waypoints
                if (waypoints.Count > 2)
                {
                    url += "&waypoints=optimize:true";
                    for (int i = 1; i < waypoints.Count - 1; i++)
                    {
                        url += $"|{waypoints[i].Latitude},{waypoints[i].Longitude}";
                    }
                }

                url += $"&key={_googleMapsApiKey}";

                // Send request
                var response = await _httpClient.GetStringAsync(url);

                // Parse response
                var document = JsonDocument.Parse(response);
                var root = document.RootElement;

                if (root.GetProperty("status").GetString() != "OK")
                {
                    // Fall back to estimated route details
                    return EstimateRouteDetails(vehicle, destinationLat, destinationLng);
                }

                // Extract route information
                var route = root.GetProperty("routes")[0];
                var legs = route.GetProperty("legs");

                double totalDistance = 0;
                double totalTime = 0;

                // Process each leg (segment between two points)
                for (int i = 0; i < legs.GetArrayLength(); i++)
                {
                    var leg = legs[i];

                    double legDistance = leg.GetProperty("distance").GetProperty("value").GetDouble() / 1000; // meters to km
                    double legTime = leg.GetProperty("duration").GetProperty("value").GetDouble() / 60; // seconds to minutes

                    totalDistance += legDistance;
                    totalTime += legTime;

                    // First leg is from vehicle to first passenger
                    if (i == 0)
                    {
                        var passenger = vehicle.Passengers[0];
                        routeDetails.AddStop(passenger.Id, passenger.Name, legDistance, legTime);
                    }
                    // Last leg is from last passenger to destination
                    else if (i == legs.GetArrayLength() - 1)
                    {
                        routeDetails.AddStop(0, "Destination", legDistance, legTime);
                    }
                    // Middle legs are between passengers
                    else
                    {
                        var passenger = vehicle.Passengers[i];
                        routeDetails.AddStop(passenger.Id, passenger.Name, legDistance, legTime);
                    }
                }

                // Set total distance and time
                routeDetails.TotalDistance = totalDistance;
                routeDetails.TotalTime = totalTime;

                // Calculate cumulative distances and times
                routeDetails.CalculateCumulatives();

                return routeDetails;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error getting route details: {ex.Message}");

                // Fall back to estimated route details
                return EstimateRouteDetails(vehicle, destinationLat, destinationLng);
            }
        }

        // Estimates route details using straight-line distances
        public RouteDetails EstimateRouteDetails(Vehicle vehicle, double destinationLat, double destinationLng)
        {
            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle));

            // Create route details object
            var routeDetails = new RouteDetails
            {
                VehicleId = vehicle.Id,
                TotalDistance = 0,
                TotalTime = 0,
                StopDetails = new List<StopDetail>()
            };

            // If no passengers, return empty route details
            if (vehicle.Passengers.Count == 0)
                return routeDetails;

            // Starting point is vehicle location
            double currentLat = vehicle.Latitude;
            double currentLng = vehicle.Longitude;

            // Add each passenger as a stop
            foreach (var passenger in vehicle.Passengers)
            {
                // Calculate distance and time from current location to passenger
                double distance = DistanceCalculator.CalculateDistance(
                    currentLat, currentLng, passenger.Latitude, passenger.Longitude);

                // Estimate time (assume 30 km/h average speed)
                double time = (distance / 30) * 60; // minutes

                // Add stop
                routeDetails.AddStop(passenger.Id, passenger.Name, distance, time);

                // Update current location
                currentLat = passenger.Latitude;
                currentLng = passenger.Longitude;
            }

            // Add final segment to destination
            double finalDistance = DistanceCalculator.CalculateDistance(
                currentLat, currentLng, destinationLat, destinationLng);
            double finalTime = (finalDistance / 30) * 60; // minutes

            routeDetails.AddStop(0, "Destination", finalDistance, finalTime);

            // Calculate cumulative distances and times
            routeDetails.CalculateCumulatives();

            return routeDetails;
        }

        // Gets a color for a route based on the route index
        public Color GetRouteColor(int index)
        {
            return _routeColors[index % _routeColors.Length];
        }

        // Geocodes an address to coordinates
        public async Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address cannot be null or empty", nameof(address));

            return await _geocodingService.GeocodeAddressAsync(address);
        }

        // Reverse geocodes coordinates to address
        public async Task<string> ReverseGeocodeAsync(double latitude, double longitude)
        {
            return await _geocodingService.ReverseGeocodeAsync(latitude, longitude);
        }

        // Gets address suggestions based on partial input
        public async Task<List<string>> GetAddressSuggestionsAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
                return new List<string>();

            return await _geocodingService.GetAddressSuggestionsAsync(query);
        }

        // IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _httpClient?.Dispose();
            }

            _disposed = true;
        }
    }
}