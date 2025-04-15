using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RideMatch.UI.Controls
{
    /// <summary>
    /// A map control that implements the IMapControl interface
    /// This is a simplified implementation that simulates a map without requiring external libraries
    /// </summary>
    public partial class MapControl : UserControl, IMapControl
    {
        // Map state
        private double _latitude = 32.0741;    // Default latitude
        private double _longitude = 34.7922;   // Default longitude
        private int _zoom = 12;                // Default zoom level
        private int _providerType = 0;         // Default provider type

        // Markers and routes
        private List<MapMarker> _markers = new List<MapMarker>();
        private List<MapRoute> _routes = new List<MapRoute>();

        // Provider color themes
        private Color[] _providerBackColors = new Color[]
        {
            Color.WhiteSmoke,     // Default/Google
            Color.FromArgb(240, 240, 240),  // OpenStreetMap
            Color.FromArgb(39, 49, 56)      // Dark mode
        };

        // Map click event
        public event EventHandler<MapClickEventArgs> MapClick;

        /// <summary>
        /// Initializes a new instance of the MapControl class
        /// </summary>
        public MapControl()
        {
            InitializeComponent();

            // Set double buffering to reduce flicker
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            // Wire up the mouse click event
            this.MouseClick += MapControl_MouseClick;
        }

        /// <summary>
        /// Initializes the map with default position
        /// </summary>
        public void Initialize(string apiKey, double latitude = 32.0741, double longitude = 34.7922)
        {
            SetPosition(latitude, longitude);
        }

        /// <summary>
        /// Sets the map position
        /// </summary>
        public void SetPosition(double latitude, double longitude, int zoom = 12)
        {
            _latitude = latitude;
            _longitude = longitude;
            _zoom = zoom;
            Refresh();
        }

        /// <summary>
        /// Changes the map provider
        /// </summary>
        public void ChangeProvider(int providerType)
        {
            if (providerType >= 0 && providerType < _providerBackColors.Length)
            {
                _providerType = providerType;
                Refresh();
            }
        }

        /// <summary>
        /// Clears all overlays
        /// </summary>
        public void ClearOverlays()
        {
            _markers.Clear();
            _routes.Clear();
            Refresh();
        }

        /// <summary>
        /// Adds a marker to the map
        /// </summary>
        public void AddMarker(double latitude, double longitude, MarkerType markerType, string tooltip = null)
        {
            _markers.Add(new MapMarker
            {
                Latitude = latitude,
                Longitude = longitude,
                MarkerType = markerType,
                Tooltip = tooltip
            });
            Refresh();
        }

        /// <summary>
        /// Adds a route to the map
        /// </summary>
        public void AddRoute(List<PointLatLng> points, string name, Color color, int width = 3)
        {
            if (points == null || points.Count < 2)
                return;

            _routes.Add(new MapRoute
            {
                Points = new List<PointLatLng>(points),
                Name = name,
                Color = color,
                Width = width
            });
            Refresh();
        }

        /// <summary>
        /// Handles mouse click events
        /// </summary>
        private void MapControl_MouseClick(object sender, MouseEventArgs e)
        {
            // Convert screen coordinates to geo coordinates
            double lat = ScreenYToLatitude(e.Y);
            double lng = ScreenXToLongitude(e.X);

            // Raise the map click event
            MapClick?.Invoke(this, new MapClickEventArgs(lat, lng));
        }

        /// <summary>
        /// Custom paint handler
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            // Fill background based on provider
            g.FillRectangle(new SolidBrush(_providerBackColors[_providerType]), ClientRectangle);

            // Draw grid to simulate a map
            DrawMapGrid(g);

            // Draw routes
            foreach (var route in _routes)
            {
                DrawRoute(g, route);
            }

            // Draw markers
            foreach (var marker in _markers)
            {
                DrawMarker(g, marker);
            }

            // Draw center coordinates
            DrawCoordinates(g);
        }

        /// <summary>
        /// Draws a simple grid to simulate a map
        /// </summary>
        private void DrawMapGrid(Graphics g)
        {
            int gridSize = 40;
            Pen gridPen = new Pen(Color.FromArgb(40, Color.Gray));

            // Draw horizontal grid lines
            for (int y = 0; y < Height; y += gridSize)
            {
                g.DrawLine(gridPen, 0, y, Width, y);
            }

            // Draw vertical grid lines
            for (int x = 0; x < Width; x += gridSize)
            {
                g.DrawLine(gridPen, x, 0, x, Height);
            }
        }

        /// <summary>
        /// Draws a route on the map
        /// </summary>
        private void DrawRoute(Graphics g, MapRoute route)
        {
            if (route.Points.Count < 2)
                return;

            // Create pen for the route
            using (Pen routePen = new Pen(route.Color, route.Width))
            {
                // Convert geo points to screen points
                Point[] screenPoints = route.Points.ConvertAll(p =>
                    new Point(
                        (int)LongitudeToScreenX(p.Longitude),
                        (int)LatitudeToScreenY(p.Latitude)
                    )).ToArray();

                // Draw the route
                g.DrawLines(routePen, screenPoints);
            }
        }

        /// <summary>
        /// Draws a marker on the map
        /// </summary>
        private void DrawMarker(Graphics g, MapMarker marker)
        {
            // Convert geo coordinates to screen coordinates
            float x = (float)LongitudeToScreenX(marker.Longitude);
            float y = (float)LatitudeToScreenY(marker.Latitude);

            // Choose marker style based on type
            Color markerColor;
            int markerSize = 10;

            switch (marker.MarkerType)
            {
                case MarkerType.Vehicle:
                    markerColor = Color.Blue;
                    markerSize = 12;
                    break;
                case MarkerType.Passenger:
                    markerColor = Color.Green;
                    markerSize = 10;
                    break;
                case MarkerType.Destination:
                    markerColor = Color.Red;
                    markerSize = 14;
                    break;
                default:
                    markerColor = Color.Purple;
                    break;
            }

            // Draw the marker
            RectangleF markerRect = new RectangleF(
                x - markerSize / 2,
                y - markerSize / 2,
                markerSize,
                markerSize);

            g.FillEllipse(new SolidBrush(markerColor), markerRect);
            g.DrawEllipse(new Pen(Color.White, 2), markerRect);

            // Draw tooltip if provided
            if (!string.IsNullOrEmpty(marker.Tooltip))
            {
                // Create tooltip background
                SizeF textSize = g.MeasureString(marker.Tooltip, Font);
                RectangleF tooltipRect = new RectangleF(
                    x + markerSize / 2 + 5,
                    y - textSize.Height / 2,
                    textSize.Width + 10,
                    textSize.Height + 6);

                // Draw tooltip background with shadow
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, Color.White)), tooltipRect);
                g.DrawRectangle(new Pen(Color.FromArgb(100, Color.Gray)), tooltipRect.X, tooltipRect.Y, tooltipRect.Width, tooltipRect.Height);

                // Draw tooltip text
                g.DrawString(marker.Tooltip, Font, new SolidBrush(Color.Black),
                    x + markerSize / 2 + 10,
                    y - textSize.Height / 2 + 3);
            }
        }

        /// <summary>
        /// Draws the current coordinates at the bottom of the map
        /// </summary>
        private void DrawCoordinates(Graphics g)
        {
            string coords = $"Center: {_latitude:F4}, {_longitude:F4} | Zoom: {_zoom}";
            SizeF textSize = g.MeasureString(coords, Font);

            // Draw background
            g.FillRectangle(new SolidBrush(Color.FromArgb(200, Color.White)),
                5, Height - textSize.Height - 10, textSize.Width + 10, textSize.Height + 5);

            // Draw text
            g.DrawString(coords, Font, Brushes.Black, 10, Height - textSize.Height - 7);
        }

        #region Coordinate Conversion Helper Methods

        /// <summary>
        /// Converts a longitude to screen X coordinate
        /// </summary>
        private double LongitudeToScreenX(double longitude)
        {
            double centerX = Width / 2.0;
            double pixelsPerDegree = Width / (360.0 / Math.Pow(2, _zoom) * 0.5);

            return centerX + (longitude - _longitude) * pixelsPerDegree;
        }

        /// <summary>
        /// Converts a latitude to screen Y coordinate
        /// </summary>
        private double LatitudeToScreenY(double latitude)
        {
            double centerY = Height / 2.0;
            double pixelsPerDegree = Height / (180.0 / Math.Pow(2, _zoom) * 0.5);

            // Note: Latitude increases as we go north, but Y coordinates increase as we go down
            return centerY - (latitude - _latitude) * pixelsPerDegree;
        }

        /// <summary>
        /// Converts a screen X coordinate to longitude
        /// </summary>
        private double ScreenXToLongitude(double x)
        {
            double centerX = Width / 2.0;
            double pixelsPerDegree = Width / (360.0 / Math.Pow(2, _zoom) * 0.5);

            return _longitude + (x - centerX) / pixelsPerDegree;
        }

        /// <summary>
        /// Converts a screen Y coordinate to latitude
        /// </summary>
        private double ScreenYToLatitude(double y)
        {
            double centerY = Height / 2.0;
            double pixelsPerDegree = Height / (180.0 / Math.Pow(2, _zoom) * 0.5);

            // Note: Latitude increases as we go north, but Y coordinates increase as we go down
            return _latitude - (y - centerY) / pixelsPerDegree;
        }

        #endregion

        private void MapControl_Load(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// Represents a marker on the map
    /// </summary>
    public class MapMarker
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public MarkerType MarkerType { get; set; }
        public string Tooltip { get; set; }
    }

    /// <summary>
    /// Represents a route on the map
    /// </summary>
    public class MapRoute
    {
        public List<PointLatLng> Points { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public int Width { get; set; }
    }
}