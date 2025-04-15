using System;

namespace RideMatch.Core.Models
{
    public class MapClickEventArgs : EventArgs
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public MapClickEventArgs(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}