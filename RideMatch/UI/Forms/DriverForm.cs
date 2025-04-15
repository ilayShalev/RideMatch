using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    public class DriverForm : BaseUserForm
    {
        private Vehicle _vehicle;
        private NumericUpDown numCapacity;
        private Button btnUpdateCapacity;

        private readonly IVehicleService _vehicleService;
        private readonly IMapService _mapService;
        private readonly IRouteService _routeService;

        public DriverForm(User user, IVehicleService vehicleService, IMapService mapService, IRouteService routeService)
            : base(user)
        {
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));

            this.Text = "RideMatch - Driver Dashboard";

            // Initialize map
            _mapService.InitializeMap(mapControl);

            // Load data when form loads
            this.Load += async (sender, e) => await LoadDataAsync();
        }

        protected override string GetInfoPanelTitle() => "Vehicle Information";

        protected override string GetDetailsPanelTitle() => "Today's Route";

        protected override string GetMapControlsTitle() => "Set Your Starting Location";

        protected override void AddInfoControls(Panel panel)
        {
            // Vehicle capacity controls
            Label lblCapacity = new Label
            {
                Text = "Vehicle Capacity:",
                Location = new Point(20, 70),
                AutoSize = true
            };
            panel.Controls.Add(lblCapacity);

            numCapacity = new NumericUpDown
            {
                Location = new Point(130, 68),
                Width = 60,
                Minimum = 1,
                Maximum = 10,
                Value = 4
            };
            panel.Controls.Add(numCapacity);

            btnUpdateCapacity = new Button
            {
                Text = "Update",
                Location = new Point(200, 66),
                Size = new Size(80, 25)
            };
            btnUpdateCapacity.Click += async (sender, e) => await UpdateVehicleCapacityAsync();
            panel.Controls.Add(btnUpdateCapacity);
        }

        protected override async Task LoadDataAsync()
        {
            try
            {
                // Show loading cursor
                this.Cursor = Cursors.WaitCursor;

                // Get vehicle data
                _vehicle = await _vehicleService.GetVehicleByUserIdAsync(_currentUser.Id);

                if (_vehicle != null)
                {
                    // Update UI with vehicle data
                    chkAvailable.Checked = _vehicle.IsAvailable;
                    numCapacity.Value = _vehicle.Capacity;

                    // Display vehicle and route on map
                    DisplayVehicleAndRouteOnMap();

                    // Update route details
                    UpdateRouteDetailsText();
                }
                else
                {
                    // No vehicle found, create one
                    DialogResult result = MessageBox.Show(
                        "No vehicle found for your account. Would you like to create one?",
                        "Create Vehicle",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Create default vehicle
                        _vehicle = new Vehicle
                        {
                            UserId = _currentUser.Id,
                            DriverName = _currentUser.Name,
                            Capacity = 4,
                            Latitude = 32.0741,
                            Longitude = 34.7922,
                            Address = "Default Location",
                            IsAvailable = false
                        };

                        int vehicleId = await _vehicleService.CreateVehicleAsync(_vehicle);
                        _vehicle.Id = vehicleId;

                        // Update UI
                        chkAvailable.Checked = _vehicle.IsAvailable;
                        numCapacity.Value = _vehicle.Capacity;

                        // Display on map
                        DisplayVehicleAndRouteOnMap();
                    }
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error loading driver data: {ex.Message}");
            }
            finally
            {
                // Reset cursor
                this.Cursor = Cursors.Default;
            }
        }

        private void DisplayVehicleAndRouteOnMap()
        {
            if (_vehicle == null)
                return;

            // Clear previous map markers
            mapControl.ClearOverlays();

            // Add vehicle marker
            mapControl.AddMarker(
                _vehicle.Latitude,
                _vehicle.Longitude,
                MarkerType.Vehicle,
                $"Vehicle: {_vehicle.DriverName} (Capacity: {_vehicle.Capacity})");

            // Center map on vehicle
            mapControl.SetPosition(_vehicle.Latitude, _vehicle.Longitude, 14);

            // Show route if available
            ShowRouteOnMap();
        }

        private async void ShowRouteOnMap()
        {
            try
            {
                if (_vehicle == null)
                    return;

                // Get today's date
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // Get route for driver
                var routeData = await _routeService.GetDriverRouteAsync(_currentUser.Id, today);

                if (routeData.Vehicle != null && routeData.Passengers != null && routeData.Passengers.Any())
                {
                    // Add passenger markers
                    foreach (var passenger in routeData.Passengers)
                    {
                        mapControl.AddMarker(
                            passenger.Latitude,
                            passenger.Longitude,
                            MarkerType.Passenger,
                            $"Passenger: {passenger.Name}" +
                            (passenger.PickupTime.HasValue ? $" (Pickup: {passenger.PickupTime.Value.ToString("HH:mm")})" : ""));
                    }

                    // Add route lines
                    List<PointLatLng> routePoints = new List<PointLatLng>();

                    // Start point (vehicle)
                    routePoints.Add(new PointLatLng { Latitude = _vehicle.Latitude, Longitude = _vehicle.Longitude });

                    // Add passenger points
                    foreach (var passenger in routeData.Passengers)
                    {
                        routePoints.Add(new PointLatLng { Latitude = passenger.Latitude, Longitude = passenger.Longitude });
                    }

                    // Draw route
                    if (routePoints.Count > 1)
                    {
                        mapControl.AddRoute(routePoints, "Today's Route", Color.Blue);
                    }
                }

                // Refresh map
                mapControl.Refresh();
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error showing route: {ex.Message}");
            }
        }

        private async void UpdateRouteDetailsText()
        {
            try
            {
                // Clear current text
                txtDetails.Text = string.Empty;

                if (_vehicle == null)
                    return;

                // Get today's date
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // Get route for driver
                var routeData = await _routeService.GetDriverRouteAsync(_currentUser.Id, today);

                if (routeData.Vehicle != null && routeData.Passengers != null)
                {
                    txtDetails.AppendText("TODAY'S ROUTE DETAILS\n", Color.Blue, true);
                    txtDetails.AppendText("====================\n\n");

                    // Vehicle info
                    txtDetails.AppendText("Vehicle: ", Color.Black, true);
                    txtDetails.AppendText($"{routeData.Vehicle.DriverName}\n");
                    txtDetails.AppendText("Capacity: ", Color.Black, true);
                    txtDetails.AppendText($"{routeData.Vehicle.Capacity}\n");
                    txtDetails.AppendText("Start Location: ", Color.Black, true);
                    txtDetails.AppendText($"{routeData.Vehicle.Address}\n\n");

                    if (routeData.Passengers.Any())
                    {
                        // Passenger list
                        txtDetails.AppendText("PASSENGERS\n", Color.Green, true);
                        txtDetails.AppendText("==========\n\n");

                        int index = 1;
                        foreach (var passenger in routeData.Passengers)
                        {
                            txtDetails.AppendText($"{index}. ", Color.Black, true);
                            txtDetails.AppendText($"{passenger.Name}\n");
                            txtDetails.AppendText("   Address: ", Color.Gray, false);
                            txtDetails.AppendText($"{passenger.Address}\n");
                            txtDetails.AppendText("   Pickup Time: ", Color.Gray, false);

                            if (passenger.PickupTime.HasValue)
                            {
                                txtDetails.AppendText($"{passenger.PickupTime.Value.ToString("HH:mm")}\n\n");
                            }
                            else
                            {
                                txtDetails.AppendText("Not scheduled\n\n");
                            }

                            index++;
                        }

                        // Route summary
                        txtDetails.AppendText("ROUTE SUMMARY\n", Color.DarkBlue, true);
                        txtDetails.AppendText("=============\n\n");
                        txtDetails.AppendText("Total Passengers: ", Color.Black, true);
                        txtDetails.AppendText($"{routeData.Passengers.Count()}\n");

                        // Get route details if available
                        var routeDetails = await _routeService.GetRouteDetailsAsync(_vehicle.Id, today);
                        if (routeDetails != null)
                        {
                            txtDetails.AppendText("Total Distance: ", Color.Black, true);
                            txtDetails.AppendText($"{routeDetails.TotalDistance:F2} km\n");
                            txtDetails.AppendText("Estimated Time: ", Color.Black, true);
                            txtDetails.AppendText($"{routeDetails.TotalTime:F0} minutes\n");
                        }
                    }
                    else
                    {
                        txtDetails.AppendText("\nNo passengers assigned to your route today.", Color.Red, false);
                    }
                }
                else
                {
                    txtDetails.AppendText("No route assigned for today.", Color.Red, true);
                }
            }
            catch (Exception ex)
            {
                txtDetails.Text = $"Error loading route details: {ex.Message}";
            }
        }

        protected override async Task UpdateAvailabilityAsync()
        {
            if (_vehicle == null)
                return;

            try
            {
                bool newAvailability = chkAvailable.Checked;

                // Update availability
                bool success = await _vehicleService.UpdateVehicleAvailabilityAsync(_vehicle.Id, newAvailability);

                if (success)
                {
                    // Update local vehicle object
                    _vehicle.IsAvailable = newAvailability;

                    DialogHelper.ShowInfo(
                        newAvailability
                            ? "You are now available for today's rides."
                            : "You are now unavailable for today's rides.");
                }
                else
                {
                    // Failed, revert checkbox
                    chkAvailable.Checked = _vehicle.IsAvailable;
                    DialogHelper.ShowError("Failed to update availability.");
                }
            }
            catch (Exception ex)
            {
                // Revert checkbox
                chkAvailable.Checked = _vehicle.IsAvailable;
                DialogHelper.ShowError($"Error updating availability: {ex.Message}");
            }
        }

        private async Task UpdateVehicleCapacityAsync()
        {
            if (_vehicle == null)
                return;

            try
            {
                int newCapacity = (int)numCapacity.Value;

                // Update capacity
                bool success = await _vehicleService.UpdateVehicleCapacityAsync(_vehicle.Id, newCapacity);

                if (success)
                {
                    // Update local vehicle object
                    _vehicle.Capacity = newCapacity;

                    DialogHelper.ShowInfo($"Vehicle capacity updated to {newCapacity}.");
                }
                else
                {
                    // Failed, revert number
                    numCapacity.Value = _vehicle.Capacity;
                    DialogHelper.ShowError("Failed to update vehicle capacity.");
                }
            }
            catch (Exception ex)
            {
                // Revert number
                numCapacity.Value = _vehicle.Capacity;
                DialogHelper.ShowError($"Error updating capacity: {ex.Message}");
            }
        }

        protected override void UpdateLocationAsync(double latitude, double longitude)
        {
            if (_vehicle == null)
                return;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Get address from coordinates
                _mapService.ReverseGeocodeAsync(latitude, longitude)
                    .ContinueWith(async addressTask =>
                    {
                        string address = await addressTask;

                        // Update vehicle location
                        bool success = await _vehicleService.UpdateVehicleLocationAsync(
                            _vehicle.Id, latitude, longitude, address);

                        if (success)
                        {
                            // Update local vehicle object
                            _vehicle.Latitude = latitude;
                            _vehicle.Longitude = longitude;
                            _vehicle.Address = address;

                            // Update map
                            DisplayVehicleAndRouteOnMap();

                            DialogHelper.ShowInfo($"Location updated to: {address}");
                        }
                        else
                        {
                            DialogHelper.ShowError("Failed to update location.");
                        }

                        this.Cursor = Cursors.Default;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error updating location: {ex.Message}");
                this.Cursor = Cursors.Default;
            }
        }
    }
}