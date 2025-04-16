using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace RideMatch.Services.MapServices
{
    /// <summary>
    /// Service for geocoding addresses and coordinates
    /// </summary>
    public class GeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _googleMapsApiKey;

        /// <summary>
        /// Initializes a new instance of the GeocodingService
        /// </summary>
        /// <param name="googleMapsApiKey">Google Maps API key</param>
        public GeocodingService(string googleMapsApiKey = null)
        {
            _googleMapsApiKey = googleMapsApiKey ?? "";
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Geocodes an address to coordinates
        /// </summary>
        /// <param name="address">The address to geocode</param>
        /// <returns>The latitude and longitude coordinates, or null if not found</returns>
        public async Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrEmpty(address))
                return null;

            try
            {
                // Encode address for URL
                string encodedAddress = HttpUtility.UrlEncode(address);

                // Build Google Geocoding API URL
                string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_googleMapsApiKey}";

                // Send request
                string response = await _httpClient.GetStringAsync(url);

                // Parse response
                JsonDocument document = JsonDocument.Parse(response);
                var root = document.RootElement;

                string status = root.GetProperty("status").GetString();
                if (status != "OK")
                    return null;

                // Extract first result
                var result = root.GetProperty("results")[0];
                var location = result.GetProperty("geometry").GetProperty("location");

                double lat = location.GetProperty("lat").GetDouble();
                double lng = location.GetProperty("lng").GetDouble();

                return (lat, lng);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error geocoding address: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Reverse geocodes coordinates to address
        /// </summary>
        /// <param name="latitude">The latitude</param>
        /// <param name="longitude">The longitude</param>
        /// <returns>The address, or empty string if not found</returns>
        public async Task<string> ReverseGeocodeAsync(double latitude, double longitude)
        {
            try
            {
                // Build Google Reverse Geocoding API URL
                string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={_googleMapsApiKey}";

                // Send request
                string response = await _httpClient.GetStringAsync(url);

                // Parse response
                JsonDocument document = JsonDocument.Parse(response);
                var root = document.RootElement;

                string status = root.GetProperty("status").GetString();
                if (status != "OK")
                    return string.Empty;

                // Extract first result
                var result = root.GetProperty("results")[0];
                string formattedAddress = result.GetProperty("formatted_address").GetString();

                return formattedAddress;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reverse geocoding: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets address suggestions for autocomplete
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>A list of address suggestions</returns>
        public async Task<List<string>> GetAddressSuggestionsAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
                return new List<string>();

            try
            {
                // Encode query for URL
                string encodedQuery = HttpUtility.UrlEncode(query);

                // Build Google Places Autocomplete API URL
                string url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={encodedQuery}&types=address&key={_googleMapsApiKey}";

                // Send request
                string response = await _httpClient.GetStringAsync(url);

                // Parse response
                JsonDocument document = JsonDocument.Parse(response);
                var root = document.RootElement;

                string status = root.GetProperty("status").GetString();
                if (status != "OK")
                    return new List<string>();

                // Extract predictions
                var predictions = root.GetProperty("predictions");
                var suggestions = new List<string>();

                foreach (var prediction in predictions.EnumerateArray())
                {
                    string description = prediction.GetProperty("description").GetString();
                    suggestions.Add(description);
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting address suggestions: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Geocodes multiple addresses to coordinates
        /// </summary>
        /// <param name="addresses">The addresses to geocode</param>
        /// <returns>A dictionary of addresses and their coordinates</returns>
        public async Task<Dictionary<string, (double Latitude, double Longitude)?>> GeocodeMultipleAddressesAsync(IEnumerable<string> addresses)
        {
            var results = new Dictionary<string, (double Latitude, double Longitude)?>();

            foreach (var address in addresses)
            {
                results[address] = await GeocodeAddressAsync(address);

                // Respect rate limits by waiting
                await Task.Delay(200);
            }

            return results;
        }
    }
}