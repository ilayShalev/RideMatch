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
    public class PassengerForm : Form
    {
        // Initializes UI components
        private void InitializeComponent();

        // Sets up UI controls
        private void SetupUI();

        // Adds location setting controls
        private void AddLocationSettingControls();

        // Enables map location selection
        private void EnableMapLocationSelection();

        // Handles map clicks to set location
        private void MapClickToSetLocation(object sender, MouseEventArgs e);

        // Searches for an address
        private async Task SearchAddressAsync(string address);

        // Updates the passenger location
        private async void UpdatePassengerLocation(double latitude, double longitude);

        // Updates assignment details text
        private void UpdateAssignmentDetailsText();

        // Loads passenger data
        private async Task LoadPassengerDataAsync();

        // Displays the passenger on the map
        private void DisplayPassengerOnMap();

        // Updates passenger availability
        private async Task UpdateAvailabilityAsync();
    }
}
