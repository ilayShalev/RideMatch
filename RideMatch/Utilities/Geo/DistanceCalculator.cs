using RideMatch.Core.Models;
using System;
using System.Collections.Generic;

namespace RideMatch.Utilities.Geo
{
    public static class DistanceCalculator
    {
        // Earth radius in kilometers
        private const double EarthRadius = 6371;

        // Calculates distance between two points using Haversine formula
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Convert degrees to radians
            double lat1Rad = ToRadians(lat1);
            double lon1Rad = ToRadians(lon1);
            double lat2Rad = ToRadians(lat2);
            double lon2Rad = ToRadians(lon2);

            // Haversine formula
            double dLat = lat2Rad - lat1Rad;
            double dLon = lon2Rad - lon1Rad;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                      Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Distance in kilometers
            return EarthRadius * c;
        }

        // Calculates distance between two points
        public static double CalculateDistance(PointLatLng point1, PointLatLng point2)
        {
            return CalculateDistance(
                point1.Latitude, point1.Longitude,
                point2.Latitude, point2.Longitude);
        }

        // Calculates the total distance of a route
        public static double CalculateRouteDistance(List<PointLatLng> points)
        {
            if (points == null || points.Count < 2)
                return 0;

            double totalDistance = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                totalDistance += CalculateDistance(points[i], points[i + 1]);
            }

            return totalDistance;
        }

        // Converts degrees to radians
        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        // Checks if coordinates are valid
        public static bool IsValidLocation(double lat, double lng)
        {
            return lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180;
        }
    }
}