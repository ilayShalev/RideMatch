using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Services.MapServices
{
    public class MapService : IMapService, IDisposable
    {
        // Initializes the map with default position
        public void InitializeMap(IMapControl mapControl, double latitude = 32.0741, double longitude = 34.7922);

        // Changes the map provider type
        public void ChangeMapProvider(IMapControl mapControl, int providerType);

        // Gets directions between a list of points
        public Task<List<PointLatLng>> GetDirectionsAsync(List<PointLatLng> waypoints);

        // Gets detailed route information using Google Maps API
        public Task<RouteDetails> GetRouteDetailsAsync(Vehicle vehicle, double destinationLat, double destinationLng);

        // Estimates route details using straight-line distances
        public RouteDetails EstimateRouteDetails(Vehicle vehicle, double destinationLat, double destinationLng);

        // Gets a color for a route based on the route index
        public Color GetRouteColor(int index);

        // Geocodes an address to coordinates
        public Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address);

        // Reverse geocodes coordinates to address
        public Task<string> ReverseGeocodeAsync(double latitude, double longitude);

        // Gets address suggestions based on partial input
        public Task<List<string>> GetAddressSuggestionsAsync(string query);

        // IDisposable implementation
        public void Dispose();
 }
}
