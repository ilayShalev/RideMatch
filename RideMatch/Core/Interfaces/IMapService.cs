using RideMatch.Core.Models;
using RideMatch.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Interfaces
{
    public interface IMapService
    {
        // Initializes the map with default position
        void InitializeMap(IMapControl mapControl, double latitude = 32.0741, double longitude = 34.7922);

        // Changes the map provider type
        void ChangeMapProvider(IMapControl mapControl, int providerType);

        // Gets directions between a list of points
        Task<List<PointLatLng>> GetDirectionsAsync(List<PointLatLng> waypoints);

        // Gets detailed route information using Google Maps API
        Task<RouteDetails> GetRouteDetailsAsync(Vehicle vehicle, double destinationLat, double destinationLng);

        // Estimates route details using straight-line distances
        RouteDetails EstimateRouteDetails(Vehicle vehicle, double destinationLat, double destinationLng);

        // Geocodes an address to coordinates
        Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address);

        // Reverse geocodes coordinates to address
        Task<string> ReverseGeocodeAsync(double latitude, double longitude);

        // Gets address suggestions based on partial input
        Task<List<string>> GetAddressSuggestionsAsync(string query);
    }
}
