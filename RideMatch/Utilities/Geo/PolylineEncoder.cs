using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Utilities.Geo
{
    public static class PolylineEncoder
    {
        // Decodes a Google Maps encoded polyline string into a list of points
        public static List<PointLatLng> Decode(string encodedPoints);

        // Encodes a list of points into a Google Maps encoded polyline string
        public static string Encode(List<PointLatLng> points);

        // Encodes a single value for the polyline
       private static void EncodeValue(StringBuilder result, int value);
    }
}
