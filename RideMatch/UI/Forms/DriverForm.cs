using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    public class DriverForm : Form
    {
        // Initializes UI components
        private void InitializeComponent();

        // Sets up UI controls
        private void SetupUI();

        // Adds location setting controls
        private void AddLocationSettingControls();

        // Handles address search
        private async Task SearchAddressAsync(string address);

        // Loads driver data
        private async Task LoadDriverDataAsync();

        // Displays the vehicle and route on the map
        private void DisplayVehicleAndRouteOnMap();

        // Shows the route on the map
        private void ShowRouteOnMap();

        // Updates the route details text
        private void UpdateRouteDetailsText();

        // Updates the driver's availability
        private async Task UpdateAvailabilityAsync();

        // Updates the vehicle capacity
        private async Task UpdateVehicleCapacityAsync();

        // Enables map location selection mode
        private void EnableMapLocationSelection();

        // Handles map click events to set location
        private void GMapControl_MouseClick(object sender, MouseEventArgs e);

        // Updates the vehicle location
        private async void UpdateVehicleLocation(double latitude, double longitude);
   }
}
