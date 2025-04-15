using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RideMatch.Utilities.Geo
{
    public static class PolylineEncoder
    {
        // Decodes a Google Maps encoded polyline string into a list of points
        public static List<PointLatLng> Decode(string encodedPoints)
        {
            if (string.IsNullOrEmpty(encodedPoints))
                return new List<PointLatLng>();

            List<PointLatLng> points = new List<PointLatLng>();
            int index = 0;
            int len = encodedPoints.Length;
            int lat = 0;
            int lng = 0;

            while (index < len)
            {
                int b;
                int shift = 0;
                int result = 0;

                do
                {
                    b = encodedPoints[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);

                int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lat += dlat;

                shift = 0;
                result = 0;

                do
                {
                    b = encodedPoints[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);

                int dlng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lng += dlng;

                points.Add(new PointLatLng
                {
                    Latitude = lat * 1e-5,
                    Longitude = lng * 1e-5
                });
            }

            return points;
        }

        // Encodes a list of points into a Google Maps encoded polyline string
        public static string Encode(List<PointLatLng> points)
        {
            if (points == null || points.Count == 0)
                return string.Empty;

            StringBuilder result = new StringBuilder();

            int lat = 0;
            int lng = 0;

            foreach (var point in points)
            {
                int latRound = (int)Math.Round(point.Latitude * 1e5);
                int lngRound = (int)Math.Round(point.Longitude * 1e5);

                int dLat = latRound - lat;
                int dLng = lngRound - lng;

                lat = latRound;
                lng = lngRound;

                EncodeValue(result, dLat);
                EncodeValue(result, dLng);
            }

            return result.ToString();
        }

        // Encodes a single value for the polyline
        private static void EncodeValue(StringBuilder result, int value)
        {
            value = value < 0 ? ~(value << 1) : (value << 1);

            while (value >= 0x20)
            {
                result.Append((char)((0x20 | (value & 0x1f)) + 63));
                value >>= 5;
            }

            result.Append((char)(value + 63));
        }
    }
}