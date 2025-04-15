using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Utilities.Geo
{
    public static class DistanceCalculator
    {
        // Calculates distance between two points using Haversine formula
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

        // Calculates distance between two points
        public static double CalculateDistance(PointLatLng point1, PointLatLng point2);

        // Calculates the total distance of a route
        public static double CalculateRouteDistance(List<PointLatLng> points);

        // Converts degrees to radians
        private static double ToRadians(double degrees);

        // Checks if coordinates are valid
        public static bool IsValidLocation(double lat, double lng);
    }
}
