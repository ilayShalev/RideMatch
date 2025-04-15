using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Interfaces
{
    public interface IMapControl
    {
        // Sets the map position
        void SetPosition(double latitude, double longitude, int zoom);

        // Changes the map provider
        void ChangeProvider(int providerType);

        // Clears all overlays
        void ClearOverlays();

        // Adds a marker to the map
        void AddMarker(double latitude, double longitude, MarkerType markerType, string tooltip = null);

        // Adds a route to the map
        void AddRoute(List<PointLatLng> points, string name, Color color, int width = 3);

        // Refreshes the map
        void Refresh();

        // Map click event
        event EventHandler<MapClickEventArgs> MapClick;
    }
}
