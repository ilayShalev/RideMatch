using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Controls;
using RideMatch.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    public class DriverForm : Form
    {
        private User _currentUser;
        private Vehicle _vehicle;
        private TabControl tabControl;
        private MapControl mapControl;
        private Panel vehicleInfoPanel;
        private Panel routeInfoPanel;
        private CheckBox chkAvailable;
        private NumericUpDown numCapacity;
        private Button btnUpdateCapacity;
        private Button btnSetLocation;
        private Button btnRefresh;
        private RichTextBox txtRouteDetails;
        private AddressSearchControl addressSearchControl;

        private readonly IVehicleService _vehicleService;
        private readonly IMapService _mapService;
        private readonly IRouteService _routeService;
        private bool _locationSelectionMode = false;

        public DriverForm(User user, IVehicleService vehicleService, IMapService mapService, IRouteService routeService)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "RideMatch - Driver Dashboard";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 600);

            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(tabControl);

            // Create dashboard tab
            TabPage dashboardTab = new TabPage("Dashboard");
            tabControl.TabPages.Add(dashboardTab);

            // Create map tab
            TabPage mapTab = new TabPage("Map & Route");
            tabControl.TabPages.Add(mapTab);

            // Create settings tab
            TabPage settingsTab = new TabPage("Settings");
            tabControl.TabPages.Add(settingsTab);

            // Setup dashboard tab
            SetupDashboardTab(dashboardTab);

            // Setup map tab
            SetupMapTab(mapTab);

            // Setup settings tab
            SetupSettingsTab(settingsTab);

            // Load data when form loads
            this.Load += async (sender, e) => await LoadDriverDataAsync();
        }

        private void SetupDashboardTab(TabPage tab)
        {
            // Create welcome panel
            Panel welcomePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };
            tab.Controls.Add(welcomePanel);

            // Welcome label
            Label lblWelcome = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            welcomePanel.Controls.Add(lblWelcome);

            // Set welcome text
            lblWelcome.Text = $"Welcome, {_currentUser.Name}!";

            // Create vehicle info panel
            vehicleInfoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            tab.Controls.Add(vehicleInfoPanel);

            // Vehicle info title
            Label lblVehicleInfo = new Label
            {
                Text = "Vehicle Information",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            vehicleInfoPanel.Controls.Add(lblVehicleInfo);

            // Vehicle availability
            chkAvailable = new CheckBox
            {
                Text = "Available for today's rides",
                Location = new Point(20, 40),
                AutoSize = true
            };
            chkAvailable.CheckedChanged += async (sender, e) => await UpdateAvailabilityAsync();
            vehicleInfoPanel.Controls.Add(chkAvailable);

            // Vehicle capacity
            Label lblCapacity = new Label
            {
                Text = "Vehicle Capacity:",
                Location = new Point(20, 70),
                AutoSize = true
            };
            vehicleInfoPanel.Controls.Add(lblCapacity);

            numCapacity = new NumericUpDown
            {
                Location = new Point(130, 68),
                Width = 60,
                Minimum = 1,
                Maximum = 10,
                Value = 4
            };
            vehicleInfoPanel.Controls.Add(numCapacity);

            btnUpdateCapacity = new Button
            {
                Text = "Update",
                Location = new Point(200, 66),
                Size = new Size(80, 25)
            };
            btnUpdateCapacity.Click += async (sender, e) => await UpdateVehicleCapacityAsync();
            vehicleInfoPanel.Controls.Add(btnUpdateCapacity);

            // Create route info panel
            routeInfoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            tab.Controls.Add(routeInfoPanel);

            // Route info title
            Label lblRouteInfo = new Label
            {
                Text = "Today's Route",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            routeInfoPanel.Controls.Add(lblRouteInfo);

            // Route details
            txtRouteDetails = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 10)
            };
            routeInfoPanel.Controls.Add(txtRouteDetails);

            // Refresh button
            btnRefresh = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            btnRefresh.Click += async (sender, e) => await LoadDriverDataAsync();
            routeInfoPanel.Controls.Add(btnRefresh);
        }

        private void SetupMapTab(TabPage tab)
        {
            // Create split container
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 300
            };
            tab.Controls.Add(splitContainer);

            // Left panel - Map controls
            Panel leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            splitContainer.Panel1.Controls.Add(leftPanel);

            // Map control panel title
            Label lblMapControls = new Label
            {
                Text = "Map Controls",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            leftPanel.Controls.Add(lblMapControls);

            // Add location controls
            AddLocationSettingControls(leftPanel);

            // Right panel - Map display
            Panel rightPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            splitContainer.Panel2.Controls.Add(rightPanel);

            // Map control
            mapControl = new MapControl
            {
                Dock = DockStyle.Fill
            };
            rightPanel.Controls.Add(mapControl);

            // Initialize map
            _mapService.InitializeMap(mapControl);
        }

        private void AddLocationSettingControls(Panel panel)
        {
            // Location group box
            GroupBox locationGroup = new GroupBox
            {
                Text = "Set Your Location",
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(10)
            };
            panel.Controls.Add(locationGroup);

            // Add address search control
            addressSearchControl = new AddressSearchControl
            {
                Dock = DockStyle.Top,
                Height = 80
            };
            addressSearchControl.AddressFound += async (sender, e) =>
            {
                await UpdateVehicleLocation(e.Latitude, e.Longitude);
            };
            locationGroup.Controls.Add(addressSearchControl);

            // Or label
            Label lblOr = new Label
            {
                Text = "-- OR --",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 20
            };
            locationGroup.Controls.Add(lblOr);

            // Set location by map button
            btnSetLocation = new Button
            {
                Text = "Set Location by Clicking on Map",
                Dock = DockStyle.Top,
                Height = 30
            };
            btnSetLocation.Click += (sender, e) => EnableMapLocationSelection();
            locationGroup.Controls.Add(btnSetLocation);

            // Instructions label
            Label lblInstructions = new Label
            {
                Text = "Click on the map to set your starting location.",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.TopLeft
            };
            locationGroup.Controls.Add(lblInstructions);
        }

        private void SetupSettingsTab(TabPage tab)
        {
            // Create settings panel
            Panel settingsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            tab.Controls.Add(settingsPanel);

            // Settings title
            Label lblSettings = new Label
            {
                Text = "Account Settings",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            settingsPanel.Controls.Add(lblSettings);

            // User info
            TableLayoutPanel userInfoTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 150,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(10)
            };
            userInfoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            userInfoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            settingsPanel.Controls.Add(userInfoTable);

            // Add user info fields
            AddUserInfoField(userInfoTable, 0, "Username:", _currentUser.Username);
            AddUserInfoField(userInfoTable, 1, "Name:", _currentUser.Name);
            AddUserInfoField(userInfoTable, 2, "Email:", _currentUser.Email);
            AddUserInfoField(userInfoTable, 3, "Phone:", _currentUser.Phone);

            // Change password button
            Button btnChangePassword = new Button
            {
                Text = "Change Password",
                Dock = DockStyle.Top,
                Height = 30,
                Width = 150,
                Margin = new Padding(0, 20, 0, 0)
            };
            btnChangePassword.Click += (sender, e) => ShowChangePasswordDialog();
            settingsPanel.Controls.Add(btnChangePassword);

            // Edit profile button
            Button btnEditProfile = new Button
            {
                Text = "Edit Profile",
                Dock = DockStyle.Top,
                Height = 30,
                Width = 150
            };
            btnEditProfile.Click += (sender, e) => ShowEditProfileDialog();
            settingsPanel.Controls.Add(btnEditProfile);

            // Logout button
            Button btnLogout = new Button
            {
                Text = "Logout",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            btnLogout.Click += (sender, e) => Logout();
            settingsPanel.Controls.Add(btnLogout);
        }

        private void AddUserInfoField(TableLayoutPanel table, int row, string label, string value)
        {
            table.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            table.Controls.Add(new Label { Text = value ?? "", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 1, row);
        }

        private async Task LoadDriverDataAsync()
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
                txtRouteDetails.Text = string.Empty;

                if (_vehicle == null)
                    return;

                // Get today's date
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // Get route for driver
                var routeData = await _routeService.GetDriverRouteAsync(_currentUser.Id, today);

                if (routeData.Vehicle != null && routeData.Passengers != null)
                {
                    txtRouteDetails.AppendText("TODAY'S ROUTE DETAILS\n", Color.Blue, true);
                    txtRouteDetails.AppendText("====================\n\n");

                    // Vehicle info
                    txtRouteDetails.AppendText("Vehicle: ", Color.Black, true);
                    txtRouteDetails.AppendText($"{routeData.Vehicle.DriverName}\n");
                    txtRouteDetails.AppendText("Capacity: ", Color.Black, true);
                    txtRouteDetails.AppendText($"{routeData.Vehicle.Capacity}\n");
                    txtRouteDetails.AppendText("Start Location: ", Color.Black, true);
                    txtRouteDetails.AppendText($"{routeData.Vehicle.Address}\n\n");

                    if (routeData.Passengers.Any())
                    {
                        // Passenger list
                        txtRouteDetails.AppendText("PASSENGERS\n", Color.Green, true);
                        txtRouteDetails.AppendText("==========\n\n");

                        int index = 1;
                        foreach (var passenger in routeData.Passengers)
                        {
                            txtRouteDetails.AppendText($"{index}. ", Color.Black, true);
                            txtRouteDetails.AppendText($"{passenger.Name}\n");
                            txtRouteDetails.AppendText("   Address: ", Color.Gray, false);
                            txtRouteDetails.AppendText($"{passenger.Address}\n");
                            txtRouteDetails.AppendText("   Pickup Time: ", Color.Gray, false);

                            if (passenger.PickupTime.HasValue)
                            {
                                txtRouteDetails.AppendText($"{passenger.PickupTime.Value.ToString("HH:mm")}\n\n");
                            }
                            else
                            {
                                txtRouteDetails.AppendText("Not scheduled\n\n");
                            }

                            index++;
                        }

                        // Route summary
                        txtRouteDetails.AppendText("ROUTE SUMMARY\n", Color.DarkBlue, true);
                        txtRouteDetails.AppendText("=============\n\n");
                        txtRouteDetails.AppendText("Total Passengers: ", Color.Black, true);
                        txtRouteDetails.AppendText($"{routeData.Passengers.Count()}\n");

                        // Get route details if available
                        var routeDetails = await _routeService.GetRouteDetailsAsync(_vehicle.Id, today);
                        if (routeDetails != null)
                        {
                            txtRouteDetails.AppendText("Total Distance: ", Color.Black, true);
                            txtRouteDetails.AppendText($"{routeDetails.TotalDistance:F2} km\n");
                            txtRouteDetails.AppendText("Estimated Time: ", Color.Black, true);
                            txtRouteDetails.AppendText($"{routeDetails.TotalTime:F0} minutes\n");
                        }
                    }
                    else
                    {
                        txtRouteDetails.AppendText("\nNo passengers assigned to your route today.", Color.Red, false);
                    }
                }
                else
                {
                    txtRouteDetails.AppendText("No route assigned for today.", Color.Red, true);
                }
            }
            catch (Exception ex)
            {
                txtRouteDetails.Text = $"Error loading route details: {ex.Message}";
            }
        }

        private async Task UpdateAvailabilityAsync()
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

        private void EnableMapLocationSelection()
        {
            // Enable location selection mode
            _locationSelectionMode = true;

            // Change button text
            btnSetLocation.Text = "Click on map to set location...";
            btnSetLocation.BackColor = Color.LightBlue;

            // Show instructions
            DialogHelper.ShowInfo("Click on the map to set your location.");

            // Subscribe to map click event
            mapControl.MapClick += GMapControl_MouseClick;
        }

        private void GMapControl_MouseClick(object sender, MapClickEventArgs e)
        {
            if (_locationSelectionMode)
            {
                // Disable location selection mode
                _locationSelectionMode = false;

                // Reset button
                btnSetLocation.Text = "Set Location by Clicking on Map";
                btnSetLocation.BackColor = SystemColors.Control;

                // Unsubscribe from event
                mapControl.MapClick -= GMapControl_MouseClick;

                // Update vehicle location
                UpdateVehicleLocation(e.Latitude, e.Longitude);
            }
        }

        private async void UpdateVehicleLocation(double latitude, double longitude)
        {
            if (_vehicle == null)
                return;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Get address from coordinates
                string address = await _mapService.ReverseGeocodeAsync(latitude, longitude);

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
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error updating location: {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async Task SearchAddressAsync(string address)
        {
            try
            {
                // Get coordinates from address
                var coordinates = await _mapService.GeocodeAddressAsync(address);

                if (coordinates.HasValue)
                {
                    // Update vehicle location
                    await UpdateVehicleLocation(coordinates.Value.Latitude, coordinates.Value.Longitude);
                }
                else
                {
                    DialogHelper.ShowError("Address not found.");
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error searching address: {ex.Message}");
            }
        }

        private void ShowChangePasswordDialog()
        {
            // This would be implemented to show a dialog for changing password
            DialogHelper.ShowInfo("Change password functionality would be implemented here.");
        }

        private void ShowEditProfileDialog()
        {
            // This would be implemented to show a dialog for editing profile
            DialogHelper.ShowInfo("Edit profile functionality would be implemented here.");
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Close form, which will trigger the FormClosed event
                // and return to login form
                this.Close();
            }
        }
    }

    // Helper extension method for RichTextBox to append colored text
    public static class RichTextBoxExtensions
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