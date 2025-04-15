using RideMatch.Core.Models;
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
    public class RouteDetailsControl : UserControl
    {
        // Initializes components
        private void InitializeComponent();

        // Displays route details for a vehicle
        public void DisplayRouteDetails(Vehicle vehicle, RouteDetails routeDetails);

        // Clears the display
        public void Clear();

        // Updates with new route details
        public void UpdateRouteDetails(Dictionary<int, RouteDetails> routeDetails);
    }
}
