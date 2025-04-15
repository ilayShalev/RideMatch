using System.Collections.Generic;
using System.Threading.Tasks;

namespace RideMatch.Services.MapServices
{
    public class GeocodingService
    {
        // Geocodes an address to coordinates
        public Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address);

        // Reverse geocodes coordinates to address
        public Task<string> ReverseGeocodeAsync(double latitude, double longitude);

        // Gets address suggestions for autocomplete
        public Task<List<string>> GetAddressSuggestionsAsync(string query);
    }
}
