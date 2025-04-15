using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Controls;
using RideMatch.UI.Helpers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    public class PassengerForm : Form
    {
        private User _currentUser;
        private Passenger _passenger;
        private TabControl tabControl;
        private MapControl mapControl;
        private Panel passengerInfoPanel;
        private Panel assignmentInfoPanel;
        private CheckBox chkAvailable;
        private Button btnSetLocation;
        private Button btnRefresh;
        private RichTextBox txtAssignmentDetails;
        private AddressSearchControl addressSearchControl;

        private readonly IPassengerService _passengerService;
        private readonly IMapService _mapService;
        private readonly IRouteService _routeService;
        private bool _locationSelectionMode = false;

        public PassengerForm(User user, IPassengerService passengerService, IMapService mapService, IRouteService routeService)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _passengerService = passengerService ?? throw new ArgumentNullException(nameof(passengerService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "RideMatch - Passenger Dashboard";
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
            TabPage mapTab = new TabPage("Map & Location");
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
            this.Load += async (sender, e) => await LoadPassengerDataAsync();
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

            // Create passenger info panel
            passengerInfoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            tab.Controls.Add(passengerInfoPanel);

            // Passenger info title
            Label lblPassengerInfo = new Label
            {
                Text = "Ride Request",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            passengerInfoPanel.Controls.Add(lblPassengerInfo);

            // Passenger availability
            chkAvailable = new CheckBox
            {
                Text = "Available for today's rides",
                Location = new Point(20, 40),
                AutoSize = true
            };
            chkAvailable.CheckedChanged += async (sender, e) => await UpdateAvailabilityAsync();
            passengerInfoPanel.Controls.Add(chkAvailable);

            // Create assignment info panel
            assignmentInfoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            tab.Controls.Add(assignmentInfoPanel);

            // Assignment info title
            Label lblAssignmentInfo = new Label
            {
                Text = "Ride Assignment",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            assignmentInfoPanel.Controls.Add(lblAssignmentInfo);

            // Assignment details
            txtAssignmentDetails = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 10)
            };
            assignmentInfoPanel.Controls.Add(txtAssignmentDetails);

            // Refresh button
            btnRefresh = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            btnRefresh.Click += async (sender, e) => await LoadPassengerDataAsync();
            assignmentInfoPanel.Controls.Add(btnRefresh);
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
                Text = "Set Your Pickup Location",
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
                Text = "Set Your Pickup Location",
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
                await UpdatePassengerLocation(e.Latitude, e.Longitude);
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
                Text = "Click on the map to set your pickup location.",
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

        private async Task LoadPassengerDataAsync()
        {
            try
            {
                // Show loading cursor
                this.Cursor = Cursors.WaitCursor;

                // Get passenger data
                _passenger = await _passengerService.GetPassengerByUserIdAsync(_currentUser.Id);

                if (_passenger != null)
                {
                    // Update UI with passenger data
                    chkAvailable.Checked = _passenger.IsAvailable;

                    // Display passenger on map
                    DisplayPassengerOnMap();

                    // Update assignment details
                    UpdateAssignmentDetailsText();
                }
                else
                {
                    // No passenger found, create one
                    DialogResult result = MessageBox.Show(
                        "No passenger profile found for your account. Would you like to create one?",
                        "Create Passenger Profile",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Create default passenger
                        _passenger = new Passenger
                        {
                            UserId = _currentUser.Id,
                            Name = _currentUser.Name,
                            Latitude = 32.0741,
                            Longitude = 34.7922,
                            Address = "Default Location",
                            IsAvailable = false
                        };

                        int passengerId = await _passengerService.CreatePassengerAsync(_passenger);
                        _passenger.Id = passengerId;

                        // Update UI
                        chkAvailable.Checked = _passenger.IsAvailable;

                        // Display on map
                        DisplayPassengerOnMap();
                    }
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error loading passenger data: {ex.Message}");
            }
            finally
            {
                // Reset cursor
                this.Cursor = Cursors.Default;
            }
        }

        private void DisplayPassengerOnMap()
        {
            if (_passenger == null)
                return;

            // Clear previous map markers
            mapControl.ClearOverlays();

            // Add passenger marker
            mapControl.AddMarker(
                _passenger.Latitude,
                _passenger.Longitude,
                MarkerType.Passenger,
                $"Pickup Location: {_passenger.Address}");

            // Center map on passenger
            mapControl.SetPosition(_passenger.Latitude, _passenger.Longitude, 14);

            // Refresh map
            mapControl.Refresh();
        }

        private async void UpdateAssignmentDetailsText()
        {
            try
            {
                // Clear current text
                txtAssignmentDetails.Text = string.Empty;

                if (_passenger == null)
                    return;

                // Get today's date
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // Get assignment for passenger
                var assignment = await _routeService.GetPassengerAssignmentAsync(_currentUser.Id, today);

                txtAssignmentDetails.AppendText("TODAY'S RIDE ASSIGNMENT\n", Color.Blue, true);
                txtAssignmentDetails.AppendText("=======================\n\n");

                if (assignment.AssignedVehicle != null)
                {
                    // Display assignment details
                    txtAssignmentDetails.AppendText("Status: ", Color.Black, true);
                    txtAssignmentDetails.AppendText("ASSIGNED\n\n", Color.Green, true);

                    // Driver info
                    txtAssignmentDetails.AppendText("Driver: ", Color.Black, true);
                    txtAssignmentDetails.AppendText($"{assignment.AssignedVehicle.DriverName}\n");

                    // Pickup time
                    txtAssignmentDetails.AppendText("Pickup Time: ", Color.Black, true);
                    if (assignment.PickupTime.HasValue)
                    {
                        txtAssignmentDetails.AppendText($"{assignment.PickupTime.Value.ToString("HH:mm")}\n");
                    }
                    else
                    {
                        txtAssignmentDetails.AppendText("Not scheduled yet\n");
                    }

                    // Vehicle info
                    txtAssignmentDetails.AppendText("\nVEHICLE DETAILS\n", Color.DarkBlue, true);
                    txtAssignmentDetails.AppendText("---------------\n");
                    txtAssignmentDetails.AppendText("Vehicle Capacity: ", Color.Black, false);
                    txtAssignmentDetails.AppendText($"{assignment.AssignedVehicle.Capacity} passengers\n\n");

                    // Pickup location
                    txtAssignmentDetails.AppendText("PICKUP LOCATION\n", Color.DarkBlue, true);
                    txtAssignmentDetails.AppendText("---------------\n");
                    txtAssignmentDetails.AppendText($"{_passenger.Address}\n");

                    // Instructions
                    txtAssignmentDetails.AppendText("\nINSTRUCTIONS\n", Color.DarkBlue, true);
                    txtAssignmentDetails.AppendText("------------\n");
                    txtAssignmentDetails.AppendText("Please be at your pickup location 5 minutes before the scheduled time.\n");
                    txtAssignmentDetails.AppendText("Contact the driver if you need to cancel or reschedule.\n");
                }
                else
                {
                    // No assignment
                    txtAssignmentDetails.AppendText("Status: ", Color.Black, true);

                    if (_passenger.IsAvailable)
                    {
                        txtAssignmentDetails.AppendText("PENDING\n\n", Color.Orange, true);
                        txtAssignmentDetails.AppendText("Your ride request is pending assignment.\n");
                        txtAssignmentDetails.AppendText("Please check back later or use the Refresh button.\n\n");
                        txtAssignmentDetails.AppendText("Your pickup location: ", Color.Black, true);
                        txtAssignmentDetails.AppendText($"{_passenger.Address}\n");
                    }
                    else
                    {
                        txtAssignmentDetails.AppendText("NOT REQUESTED\n\n", Color.Red, true);
                        txtAssignmentDetails.AppendText("You have not requested a ride for today.\n");
                        txtAssignmentDetails.AppendText("To request a ride, check the 'Available for today's rides' box.\n");
                    }
                }
            }
            catch (Exception ex)
            {
                txtAssignmentDetails.Text = $"Error loading assignment details: {ex.Message}";
            }
        }

        private async Task UpdateAvailabilityAsync()
        {
            if (_passenger == null)
                return;

            try
            {
                bool newAvailability = chkAvailable.Checked;

                // Update availability
                bool success = await _passengerService.UpdatePassengerAvailabilityAsync(_passenger.Id, newAvailability);

                if (success)
                {
                    // Update local passenger object
                    _passenger.IsAvailable = newAvailability;

                    // Update assignment text
                    UpdateAssignmentDetailsText();

                    DialogHelper.ShowInfo(
                        newAvailability
                            ? "Your ride request has been submitted."
                            : "Your ride request has been canceled.");
                }
                else
                {
                    // Failed, revert checkbox
                    chkAvailable.Checked = _passenger.IsAvailable;
                    DialogHelper.ShowError("Failed to update ride request status.");
                }
            }
            catch (Exception ex)
            {
                // Revert checkbox
                chkAvailable.Checked = _passenger.IsAvailable;
                DialogHelper.ShowError($"Error updating availability: {ex.Message}");
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
            DialogHelper.ShowInfo("Click on the map to set your pickup location.");

            // Subscribe to map click event
            mapControl.MapClick += MapClickToSetLocation;
        }

        private void MapClickToSetLocation(object sender, MapClickEventArgs e)
        {
            if (_locationSelectionMode)
            {
                // Disable location selection mode
                _locationSelectionMode = false;

                // Reset button
                btnSetLocation.Text = "Set Location by Clicking on Map";
                btnSetLocation.BackColor = SystemColors.Control;

                // Unsubscribe from event
                mapControl.MapClick -= MapClickToSetLocation;

                // Update passenger location
                UpdatePassengerLocation(e.Latitude, e.Longitude);
            }
        }

        private async Task UpdatePassengerLocation(double latitude, double longitude)
        {
            if (_passenger == null)
                return;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Get address from coordinates
                string address = await _mapService.ReverseGeocodeAsync(latitude, longitude);

                // Update passenger location
                bool success = await _passengerService.UpdatePassengerLocationAsync(
                    _passenger.Id, latitude, longitude, address);

                if (success)
                {
                    // Update local passenger object
                    _passenger.Latitude = latitude;
                    _passenger.Longitude = longitude;
                    _passenger.Address = address;

                    // Update map
                    DisplayPassengerOnMap();

                    // Update assignment details
                    UpdateAssignmentDetailsText();

                    DialogHelper.ShowInfo($"Pickup location updated to: {address}");
                }
                else
                {
                    DialogHelper.ShowError("Failed to update pickup location.");
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
                    // Update passenger location
                    await UpdatePassengerLocation(coordinates.Value.Latitude, coordinates.Value.Longitude);
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
}