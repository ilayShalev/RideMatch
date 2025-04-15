using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RideMatch.UI.Controls
{
    public class RouteDetailsControl : UserControl
    {
        private TabControl tabControl;
        private Dictionary<int, RichTextBox> routeTextBoxes;

        public RouteDetailsControl()
        {
            InitializeComponent();
            routeTextBoxes = new Dictionary<int, RichTextBox>();
        }

        private void InitializeComponent()
        {
            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(tabControl);

            // Add no routes tab
            TabPage noRoutesTab = new TabPage("No Routes");

            // Add label to no routes tab
            Label lblNoRoutes = new Label
            {
                Text = "No routes to display",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            noRoutesTab.Controls.Add(lblNoRoutes);

            // Add tab
            tabControl.TabPages.Add(noRoutesTab);
        }

        private void RouteDetailsControl_Load(object sender, EventArgs e)
        {
            // Nothing to do here
        }

        // Displays route details for a vehicle
        public void DisplayRouteDetails(Vehicle vehicle, RouteDetails routeDetails)
        {
            if (vehicle == null || routeDetails == null)
                return;

            // Create a new tab if it doesn't exist
            string tabName = $"Vehicle {vehicle.Id}";

            TabPage routeTab = null;
            RichTextBox rtbDetails = null;

            // Check if tab already exists
            foreach (TabPage tab in tabControl.TabPages)
            {
                if (tab.Text == tabName)
                {
                    routeTab = tab;
                    break;
                }
            }

            // Create new tab if needed
            if (routeTab == null)
            {
                routeTab = new TabPage(tabName);

                // Create rich text box
                rtbDetails = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Font = new Font("Consolas", 9)
                };
                routeTab.Controls.Add(rtbDetails);

                // Add to dictionary
                routeTextBoxes[vehicle.Id] = rtbDetails;

                // Remove "No Routes" tab if it exists
                if (tabControl.TabPages.Count == 1 && tabControl.TabPages[0].Text == "No Routes")
                {
                    tabControl.TabPages.RemoveAt(0);
                }

                // Add new tab
                tabControl.TabPages.Add(routeTab);
            }
            else
            {
                // Get existing text box
                rtbDetails = routeTextBoxes[vehicle.Id];
            }

            // Clear text box
            rtbDetails.Clear();

            // Add vehicle details
            rtbDetails.AppendText($"Driver: {vehicle.DriverName}\n", Color.Blue, true);
            rtbDetails.AppendText($"Vehicle Capacity: {vehicle.Capacity}\n", Color.Black, false);
            rtbDetails.AppendText($"Start Location: {vehicle.Address}\n\n", Color.Black, false);

            // Add route summary
            rtbDetails.AppendText("ROUTE SUMMARY\n", Color.DarkGreen, true);
            rtbDetails.AppendText("=============\n", Color.Black, false);
            rtbDetails.AppendText($"Total Distance: {routeDetails.GetTotalDistance():F2} km\n", Color.Black, false);
            rtbDetails.AppendText($"Total Time: {routeDetails.GetTotalTime():F0} minutes\n\n", Color.Black, false);

            // Add stop details
            rtbDetails.AppendText("STOPS\n", Color.DarkBlue, true);
            rtbDetails.AppendText("=====\n", Color.Black, false);

            foreach (var stop in routeDetails.StopDetails)
            {
                string passengerInfo = (stop.PassengerId == 0) ? "Destination" : $"Passenger: {stop.PassengerName}";

                rtbDetails.AppendText($"Stop {stop.StopNumber}: ", Color.Blue, true);
                rtbDetails.AppendText($"{passengerInfo}\n", Color.Black, false);

                rtbDetails.AppendText("  Distance from previous: ", Color.Gray, false);
                rtbDetails.AppendText($"{stop.DistanceFromPrevious:F2} km\n", Color.Black, false);

                rtbDetails.AppendText("  Time from previous: ", Color.Gray, false);
                rtbDetails.AppendText($"{stop.TimeFromPrevious:F0} minutes\n", Color.Black, false);

                rtbDetails.AppendText("  Cumulative distance: ", Color.Gray, false);
                rtbDetails.AppendText($"{stop.CumulativeDistance:F2} km\n", Color.Black, false);

                rtbDetails.AppendText("  Cumulative time: ", Color.Gray, false);
                rtbDetails.AppendText($"{stop.CumulativeTime:F0} minutes\n", Color.Black, false);

                if (stop.ArrivalTime.HasValue)
                {
                    rtbDetails.AppendText("  Arrival time: ", Color.Gray, false);
                    rtbDetails.AppendText($"{stop.ArrivalTime.Value.ToString("HH:mm")}\n", Color.Black, false);
                }

                rtbDetails.AppendText("\n");
            }

            // Select the tab
            tabControl.SelectedTab = routeTab;
        }

        // Clears the display
        public void Clear()
        {
            // Clear all tabs
            tabControl.TabPages.Clear();
            routeTextBoxes.Clear();

            // Add no routes tab
            TabPage noRoutesTab = new TabPage("No Routes");

            // Add label to no routes tab
            Label lblNoRoutes = new Label
            {
                Text = "No routes to display",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            noRoutesTab.Controls.Add(lblNoRoutes);

            // Add tab
            tabControl.TabPages.Add(noRoutesTab);
        }

        // Updates with new route details
        public void UpdateRouteDetails(Dictionary<int, RouteDetails> routeDetails)
        {
            if (routeDetails == null || routeDetails.Count == 0)
            {
                Clear();
                return;
            }

            // Get vehicle information for each route
            Dictionary<int, Vehicle> vehicles = new Dictionary<int, Vehicle>();

            // For each route detail, display it
            foreach (var kvp in routeDetails)
            {
                int vehicleId = kvp.Key;
                RouteDetails details = kvp.Value;

                // Create a simple vehicle object with ID
                Vehicle vehicle = new Vehicle { Id = vehicleId, DriverName = $"Driver {vehicleId}" };

                // Display route details
                DisplayRouteDetails(vehicle, details);
            }
        }
    }

    // Extension method for rich text box
    public static class RichTextBoxExtensions2
    {
        public static void AppendText(this RichTextBox box, string text, Color color, bool bold)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.SelectionFont = new Font(box.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}