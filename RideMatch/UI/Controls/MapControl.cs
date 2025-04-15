using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Controls
{
    public class MapControl : IMapControl
    {
        // Initializes the map with Google Maps
        public void Initialize(string apiKey, double latitude = 32.0741, double longitude = 34.7922);

        // Sets the map position
        public void SetPosition(double latitude, double longitude, int zoom = 12);

        // Changes the map provider
        public void ChangeProvider(int providerType);

        // Clears all overlays
        public void ClearOverlays();

        // Adds a marker to the map
        public void AddMarker(double latitude, double longitude, MarkerType markerType, string tooltip = null);

        // Adds a route to the map
        public void AddRoute(List<PointLatLng> points, string name, Color color, int width = 3);

        // Refreshes the map
        public void Refresh();
    
        // Handles map click events
        public event EventHandler<MapClickEventArgs> MapClick
        }

}
