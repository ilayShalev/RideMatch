using RideMatch.Core.Models;
using RideMatch.UI.Controls;
using RideMatch.UI.Helpers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    /// <summary>
    /// Base class for user-specific forms (Driver and Passenger)
    /// Eliminates duplication between these form types
    /// </summary>
    public abstract partial class BaseUserForm : Form
    {
        protected User _currentUser;
        protected TabControl tabControl;
        protected MapControl mapControl;
        protected Panel infoPanel;
        protected Panel detailsPanel;
        protected CheckBox chkAvailable;
        protected Button btnSetLocation;
        protected Button btnRefresh;
        protected RichTextBox txtDetails;
        protected AddressSearchControl addressSearchControl;
        protected bool _locationSelectionMode = false;

        public BaseUserForm(User user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            InitializeBaseComponents();
        }

        /// <summary>
        /// Initializes the base UI components common to all user forms
        /// </summary>
        protected virtual void InitializeBaseComponents()
        {
            // Form settings
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
        }

        /// <summary>
        /// Sets up the dashboard tab with welcome panel, info panel, and details panel
        /// </summary>
        protected virtual void SetupDashboardTab(TabPage tab)
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
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"Welcome, {_currentUser.Name}!"
            };
            welcomePanel.Controls.Add(lblWelcome);

            // Create info panel (for availability, etc.)
            infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            tab.Controls.Add(infoPanel);

            // Info title
            Label lblInfoTitle = new Label
            {
                Text = GetInfoPanelTitle(),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            infoPanel.Controls.Add(lblInfoTitle);

            // Availability checkbox
            chkAvailable = new CheckBox
            {
                Text = "Available for today's rides",
                Location = new Point(20, 40),
                AutoSize = true
            };
            chkAvailable.CheckedChanged += async (sender, e) => await UpdateAvailabilityAsync();
            infoPanel.Controls.Add(chkAvailable);

            // Add additional info controls
            AddInfoControls(infoPanel);

            // Create details panel
            detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            tab.Controls.Add(detailsPanel);

            // Details title
            Label lblDetailsTitle = new Label
            {
                Text = GetDetailsPanelTitle(),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            detailsPanel.Controls.Add(lblDetailsTitle);

            // Details text box
            txtDetails = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 10)
            };
            detailsPanel.Controls.Add(txtDetails);

            // Refresh button
            btnRefresh = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            btnRefresh.Click += async (sender, e) => await LoadDataAsync();
            detailsPanel.Controls.Add(btnRefresh);
        }

        /// <summary>
        /// Sets up the map tab with location controls and map display
        /// </summary>
        protected virtual void SetupMapTab(TabPage tab)
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
                Text = GetMapControlsTitle(),
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
        }

        /// <summary>
        /// Adds the location setting controls to the panel
        /// </summary>
        protected virtual void AddLocationSettingControls(Panel panel)
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
            addressSearchControl.AddressFound += AddressSearchControl_AddressFound;
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
                Text = "Click on the map to set your location.",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.TopLeft
            };
            locationGroup.Controls.Add(lblInstructions);
        }

        /// <summary>
        /// Sets up the settings tab with user info and account settings
        /// </summary>
        protected virtual void SetupSettingsTab(TabPage tab)
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

        /// <summary>
        /// Helper to add a user info field to the table
        /// </summary>
        protected void AddUserInfoField(TableLayoutPanel table, int row, string label, string value)
        {
            table.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            table.Controls.Add(new Label { Text = value ?? "", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 1, row);
        }

        /// <summary>
        /// Enables the map location selection mode
        /// </summary>
        protected virtual void EnableMapLocationSelection()
        {
            // Enable location selection mode
            _locationSelectionMode = true;

            // Change button text
            btnSetLocation.Text = "Click on map to set location...";
            btnSetLocation.BackColor = Color.LightBlue;

            // Show instructions
            DialogHelper.ShowInfo("Click on the map to set your location.");

            // Subscribe to map click event
            mapControl.MapClick += MapControl_MapClick;
        }

        /// <summary>
        /// Handles map click events for location selection
        /// </summary>
        protected virtual void MapControl_MapClick(object sender, MapClickEventArgs e)
        {
            if (_locationSelectionMode)
            {
                // Disable location selection mode
                _locationSelectionMode = false;

                // Reset button
                btnSetLocation.Text = "Set Location by Clicking on Map";
                btnSetLocation.BackColor = SystemColors.Control;

                // Unsubscribe from event
                mapControl.MapClick -= MapControl_MapClick;

                // Update location
                UpdateLocationAsync(e.Latitude, e.Longitude);
            }
        }

        /// <summary>
        /// Handles address found events from the address search control
        /// </summary>
        protected virtual void AddressSearchControl_AddressFound(object sender, AddressFoundEventArgs e)
        {
            UpdateLocationAsync(e.Latitude, e.Longitude);
        }

        /// <summary>
        /// Shows a dialog for changing password
        /// </summary>
        protected virtual void ShowChangePasswordDialog()
        {
            DialogHelper.ShowInfo("Change password functionality would be implemented here.");
        }

        /// <summary>
        /// Shows a dialog for editing profile
        /// </summary>
        protected virtual void ShowEditProfileDialog()
        {
            DialogHelper.ShowInfo("Edit profile functionality would be implemented here.");
        }

        /// <summary>
        /// Handles logout
        /// </summary>
        protected virtual void Logout()
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

        #region Abstract Methods to be implemented by derived classes

        /// <summary>
        /// Gets the title for the info panel
        /// </summary>
        protected abstract string GetInfoPanelTitle();

        /// <summary>
        /// Gets the title for the details panel
        /// </summary>
        protected abstract string GetDetailsPanelTitle();

        /// <summary>
        /// Gets the title for the map controls
        /// </summary>
        protected abstract string GetMapControlsTitle();

        /// <summary>
        /// Adds additional controls to the info panel
        /// </summary>
        protected abstract void AddInfoControls(Panel panel);

        /// <summary>
        /// Updates availability
        /// </summary>
        protected abstract Task UpdateAvailabilityAsync();

        /// <summary>
        /// Updates location
        /// </summary>
        protected abstract void UpdateLocationAsync(double latitude, double longitude);

        /// <summary>
        /// Loads data for the form
        /// </summary>
        protected abstract Task LoadDataAsync();

        #endregion
    }

    /// <summary>
    /// Extension method for RichTextBox to append colored text
    /// Consolidated from duplicate implementations
    /// </summary>
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