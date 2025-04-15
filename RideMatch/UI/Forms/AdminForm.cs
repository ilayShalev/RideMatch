using RideMatch.UI.Controls;
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
    public class AdminForm : Form
    {
        // Initializes UI components
        private void InitializeComponent();

        // Sets up the main UI
        private void SetupUI();

        // Sets up the Users tab
        private void SetupUsersTab();

        // Sets up the Drivers tab
        private void SetupDriverTab();

        // Sets up the Passengers tab
        private void SetupPassengersTab();

        // Sets up the Routes tab
        private void SetupRoutesTab();

        // Sets up the Destination tab
        private void SetupDestinationTab();

        // Sets up the Scheduling tab
        private void SetupSchedulingTab();

        // Refreshes the scheduling history list view
        private async Task RefreshHistoryListView(ListView listView);

        // Runs the scheduler manually
        private async Task RunSchedulerAsync();

        // Loads all data
        private async Task LoadAllDataAsync();

        // Loads user data
        private async Task LoadUsersAsync();

        // Loads vehicle data
        private async Task LoadVehiclesAsync();

        // Loads passenger data
        private async Task LoadPassengersAsync();

        // Displays users in the list view
        private async Task DisplayUsersAsync(ListView listView);

        // Displays drivers in the list view
        private async Task DisplayDriversAsync(ListView listView);

        // Displays passengers in the list view
        private async Task DisplayPassengersAsync(ListView listView);

        // Displays vehicles on the map
        private void DisplayVehiclesOnMap(GMapControl mapControl);

        // Displays passengers on the map
        private void DisplayPassengersOnMap(GMapControl mapControl);

        // Loads routes for a specific date
        private async Task LoadRoutesForDateAsync(DateTime date);

        // Displays route details for a specific vehicle
        private void DisplayRouteDetails(int vehicleId);

        // Updates the route details display
        private void UpdateRouteDetailsDisplay();

        // Updates the target arrival time
        private async Task UpdateTargetTimeAsync(string timeString);

        // Shows the vehicle edit form
        private void ShowVehicleEditForm(int vehicleId);

        // Shows the passenger edit form
        private void ShowPassengerEditForm(int passengerId);
    }
}
