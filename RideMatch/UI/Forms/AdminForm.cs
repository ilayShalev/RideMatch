using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.Repository;
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
    /// <summary>
    /// AdminForm provides comprehensive system management capabilities for administrators.
    /// It allows managing users, vehicles, passengers, routes, and system settings.
    /// </summary>
    public partial class AdminForm : Form
    {
        private User _currentUser;
        private TabControl tabControl;
        private DataGridView usersGridView;
        private DataGridView driversGridView;
        private DataGridView passengersGridView;
        private DataGridView routesGridView;
        private MapControl mapControl;
        private RouteDetailsControl routeDetailsControl;
        private Panel destinationPanel;
        private Panel schedulingPanel;
        private ListBox schedulingLogListBox;
        private DateTimePicker datePicker;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        // Destination controls
        private TextBox txtDestinationName;
        private TextBox txtDestinationAddress;
        private TextBox txtDestinationTargetTime;
        private Button btnUpdateDestination;
        private Button btnSetDestinationOnMap;
        private Label lblDestinationCoordinates;
        private double _destinationLatitude;
        private double _destinationLongitude;

        // Scheduling controls
        private CheckBox chkSchedulingEnabled;
        private DateTimePicker timeScheduled;
        private Button btnRunNow;
        private Button btnSaveScheduling;

        // Services
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;
        private readonly IPassengerService _passengerService;
        private readonly IMapService _mapService;
        private readonly IRouteService _routeService;
        private readonly IRoutingService _routingService;
        private readonly ISchedulerService _schedulerService;
        private readonly SettingsRepository _settingsRepository;

        private bool _locationSelectionMode = false;
        private string _currentDate;
        private Solution _currentSolution;

        /// <summary>
        /// Initializes a new instance of the AdminForm
        /// </summary>
        public AdminForm(User user, IUserService userService, IVehicleService vehicleService,
            IPassengerService passengerService, IMapService mapService, IRouteService routeService,
            IRoutingService routingService, ISchedulerService schedulerService,
            SettingsRepository settingsRepository)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _passengerService = passengerService ?? throw new ArgumentNullException(nameof(passengerService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));

            // Set current date
            _currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            InitializeComponent();
            SetupUI();

            // Set form title
            this.Text = $"RideMatch Admin Panel - {_currentUser.Name}";

            // Initialize map
            _mapService.InitializeMap(mapControl);

            // Load data when form loads
        }

        /// <summary>
        /// Initialize the components
        /// </summary>

        /// <summary>
        /// Sets up the main UI with tabs
        /// </summary>
        private void SetupUI()
        {
            // Create tabs
            TabPage usersTab = new TabPage("Users");
            TabPage driversTab = new TabPage("Drivers");
            TabPage passengersTab = new TabPage("Passengers");
            TabPage routesTab = new TabPage("Routes");
            TabPage destinationTab = new TabPage("Destination");
            TabPage schedulingTab = new TabPage("Scheduling");

            // Add tabs to the tab control
            tabControl.TabPages.AddRange(new TabPage[] {
                usersTab, driversTab, passengersTab, routesTab, destinationTab, schedulingTab
            });

            // Set up each tab
            SetupUsersTab(usersTab);
            SetupDriversTab(driversTab);
            SetupPassengersTab(passengersTab);
            SetupRoutesTab(routesTab);
            SetupDestinationTab(destinationTab);
            SetupSchedulingTab(schedulingTab);
        }

        /// <summary>
        /// Sets up the Users tab
        /// </summary>
        private void SetupUsersTab(TabPage tab)
        {
            // Create users grid
            usersGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false
            };

            // Context menu for users grid
            ContextMenuStrip usersContextMenu = new ContextMenuStrip();
            usersContextMenu.Items.Add("Edit User", null, async (s, e) => await EditSelectedUser());
            usersContextMenu.Items.Add("Delete User", null, async (s, e) => await DeleteSelectedUser());
            usersGridView.ContextMenuStrip = usersContextMenu;

            // Add grid to tab
            tab.Controls.Add(usersGridView);

            // Create buttons panel
            Panel buttonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };
            tab.Controls.Add(buttonsPanel);

            // Add user button
            Button btnAddUser = new Button
            {
                Text = "Add User",
                Dock = DockStyle.Right,
                Width = 100
            };
            btnAddUser.Click += (s, e) => ShowAddUserDialog();
            buttonsPanel.Controls.Add(btnAddUser);

            // Refresh button
            Button btnRefresh = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Right,
                Width = 100
            };
            btnRefresh.Click += async (s, e) => await LoadUsersAsync();
            buttonsPanel.Controls.Add(btnRefresh);
        }

        /// <summary>
        /// Sets up the Drivers tab
        /// </summary>
        private void SetupDriversTab(TabPage tab)
        {
            // Split container for grid and map
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 400
            };
            tab.Controls.Add(splitContainer);

            // Create drivers grid
            driversGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false
            };
            driversGridView.SelectionChanged += (s, e) => ShowSelectedDriverOnMap();
            splitContainer.Panel1.Controls.Add(driversGridView);

            // Create map panel for right side
            Panel mapPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            splitContainer.Panel2.Controls.Add(mapPanel);

            // Create map controls
            mapControl = new MapControl
            {
                Dock = DockStyle.Fill
            };
            mapPanel.Controls.Add(mapControl);
        }

        /// <summary>
        /// Sets up the Passengers tab
        /// </summary>
        private void SetupPassengersTab(TabPage tab)
        {
            // Create passengers grid
            passengersGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false
            };

            // Context menu for passengers grid
            ContextMenuStrip passengersContextMenu = new ContextMenuStrip();
            passengersContextMenu.Items.Add("View on Map", null, (s, e) => ShowSelectedPassengerOnMap());
            passengersContextMenu.Items.Add("Edit Passenger", null, (s, e) => EditSelectedPassenger());
            passengersGridView.ContextMenuStrip = passengersContextMenu;

            // Add grid to tab
            tab.Controls.Add(passengersGridView);

            // Create buttons panel
            Panel buttonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };
            tab.Controls.Add(buttonsPanel);

            // Refresh button
            Button btnRefresh = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Right,
                Width = 100
            };
            btnRefresh.Click += async (s, e) => await LoadPassengersAsync();
            buttonsPanel.Controls.Add(btnRefresh);

            // Show on map button
            Button btnShowOnMap = new Button
            {
                Text = "Show on Map",
                Dock = DockStyle.Right,
                Width = 100
            };
            btnShowOnMap.Click += (s, e) => ShowSelectedPassengerOnMap();
            buttonsPanel.Controls.Add(btnShowOnMap);
        }

        /// <summary>
        /// Sets up the Routes tab
        /// </summary>
        private void SetupRoutesTab(TabPage tab)
        {
            // Split container for top and bottom sections
            SplitContainer mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200
            };
            tab.Controls.Add(mainSplitContainer);

            // Top panel with date picker and route list
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            mainSplitContainer.Panel1.Controls.Add(topPanel);

            // Date picker container
            Panel datePickerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(5)
            };
            topPanel.Controls.Add(datePickerPanel);

            // Date label
            Label lblDate = new Label
            {
                Text = "Date:",
                Dock = DockStyle.Left,
                Width = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };
            datePickerPanel.Controls.Add(lblDate);

            // Date picker
            datePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Dock = DockStyle.Left,
                Width = 100
            };
            datePicker.ValueChanged += async (s, e) =>
            {
                _currentDate = datePicker.Value.ToString("yyyy-MM-dd");
                await LoadRoutesForDateAsync(datePicker.Value);
            };
            datePickerPanel.Controls.Add(datePicker);

            // Generate button
            Button btnGenerateRoutes = new Button
            {
                Text = "Generate Routes",
                Dock = DockStyle.Right,
                Width = 120
            };
            btnGenerateRoutes.Click += async (s, e) => await GenerateRoutesAsync();
            datePickerPanel.Controls.Add(btnGenerateRoutes);

            // Routes grid
            routesGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false
            };
            routesGridView.SelectionChanged += async (s, e) => await DisplaySelectedRoute();
            topPanel.Controls.Add(routesGridView);

            // Bottom split container for map and details
            SplitContainer bottomSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 250
            };
            mainSplitContainer.Panel2.Controls.Add(bottomSplitContainer);

            // Map panel
            Panel mapPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            bottomSplitContainer.Panel1.Controls.Add(mapPanel);

            // Use the map control already created
            mapPanel.Controls.Add(mapControl);

            // Route details panel
            routeDetailsControl = new RouteDetailsControl
            {
                Dock = DockStyle.Fill
            };
            bottomSplitContainer.Panel2.Controls.Add(routeDetailsControl);
        }

        /// <summary>
        /// Sets up the Destination tab
        /// </summary>
        private void SetupDestinationTab(TabPage tab)
        {
            // Split container for destination form and map
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 300
            };
            tab.Controls.Add(splitContainer);

            // Destination form panel
            destinationPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            splitContainer.Panel1.Controls.Add(destinationPanel);

            // Destination title
            Label lblDestTitle = new Label
            {
                Text = "Destination Settings",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            destinationPanel.Controls.Add(lblDestTitle);

            // Create form layout for destination
            TableLayoutPanel destTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(0, 10, 0, 0)
            };
            destTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            destTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            destinationPanel.Controls.Add(destTable);

            // Name
            destTable.Controls.Add(new Label { Text = "Name:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            txtDestinationName = new TextBox { Dock = DockStyle.Fill };
            destTable.Controls.Add(txtDestinationName, 1, 0);

            // Address
            destTable.Controls.Add(new Label { Text = "Address:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            txtDestinationAddress = new TextBox { Dock = DockStyle.Fill };
            destTable.Controls.Add(txtDestinationAddress, 1, 1);

            // Coordinates
            destTable.Controls.Add(new Label { Text = "Coordinates:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            lblDestinationCoordinates = new Label { Dock = DockStyle.Fill, Text = "0.000, 0.000" };
            destTable.Controls.Add(lblDestinationCoordinates, 1, 2);

            // Target Time
            destTable.Controls.Add(new Label { Text = "Target Time:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            txtDestinationTargetTime = new TextBox { Dock = DockStyle.Fill, Text = "09:00" };
            destTable.Controls.Add(txtDestinationTargetTime, 1, 3);

            // Buttons
            Panel buttonPanel = new Panel { Dock = DockStyle.Fill };
            destTable.Controls.Add(buttonPanel, 1, 4);

            btnSetDestinationOnMap = new Button
            {
                Text = "Set Location on Map",
                Width = 150,
                Height = 30,
                Location = new Point(0, 0)
            };
            btnSetDestinationOnMap.Click += (s, e) => EnableDestinationLocationSelection();
            buttonPanel.Controls.Add(btnSetDestinationOnMap);

            btnUpdateDestination = new Button
            {
                Text = "Update Destination",
                Width = 150,
                Height = 30,
                Location = new Point(0, 40)
            };
            btnUpdateDestination.Click += async (s, e) => await UpdateDestinationAsync();
            buttonPanel.Controls.Add(btnUpdateDestination);

            // Map panel for right side (reusing the map control)
            Panel mapPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            splitContainer.Panel2.Controls.Add(mapPanel);
            // We will add the map control when switching to this tab
        }

        /// <summary>
        /// Sets up the Scheduling tab
        /// </summary>
        private void SetupSchedulingTab(TabPage tab)
        {
            // Main layout
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(layout);

            // Settings panel
            schedulingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };
            layout.Controls.Add(schedulingPanel, 0, 0);

            // Scheduling title
            Label lblSchedulingTitle = new Label
            {
                Text = "Scheduling Settings",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(200, 25)
            };
            schedulingPanel.Controls.Add(lblSchedulingTitle);

            // Enable scheduling checkbox
            chkSchedulingEnabled = new CheckBox
            {
                Text = "Enable Automatic Scheduling",
                Location = new Point(20, 45),
                AutoSize = true
            };
            schedulingPanel.Controls.Add(chkSchedulingEnabled);

            // Time label
            Label lblTime = new Label
            {
                Text = "Time:",
                Location = new Point(20, 75),
                Size = new Size(40, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            schedulingPanel.Controls.Add(lblTime);

            // Time picker
            timeScheduled = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Today.AddHours(7), // Default 7:00 AM
                Location = new Point(70, 75),
                Width = 100
            };
            schedulingPanel.Controls.Add(timeScheduled);

            // Save button
            btnSaveScheduling = new Button
            {
                Text = "Save Settings",
                Location = new Point(20, 105),
                Size = new Size(120, 30)
            };
            btnSaveScheduling.Click += async (s, e) => await SaveSchedulingSettingsAsync();
            schedulingPanel.Controls.Add(btnSaveScheduling);

            // Run now button
            btnRunNow = new Button
            {
                Text = "Run Now",
                Location = new Point(150, 105),
                Size = new Size(120, 30)
            };
            btnRunNow.Click += async (s, e) => await RunSchedulerAsync();
            schedulingPanel.Controls.Add(btnRunNow);

            // Log panel
            Panel logPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };
            layout.Controls.Add(logPanel, 0, 1);

            // Log title
            Label lblLogTitle = new Label
            {
                Text = "Scheduling Log",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            logPanel.Controls.Add(lblLogTitle);

            // Log list box
            schedulingLogListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9)
            };
            logPanel.Controls.Add(schedulingLogListBox);
        }

        /// <summary>
        /// Handles tab selection change to update the UI accordingly
        /// </summary>
        private async void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTab = tabControl.SelectedTab;
            if (selectedTab == null) return;

            switch (selectedTab.Text)
            {
                case "Users":
                    await LoadUsersAsync();
                    break;
                case "Drivers":
                    await LoadVehiclesAsync();
                    break;
                case "Passengers":
                    await LoadPassengersAsync();
                    break;
                case "Routes":
                    await LoadRoutesForDateAsync(datePicker.Value);
                    break;
                case "Destination":
                    await LoadDestinationAsync();
                    break;
                case "Scheduling":
                    await LoadSchedulingSettingsAsync();
                    await RefreshHistoryListView();
                    break;
            }
        }

        #region Data Loading Methods

        /// <summary>
        /// Loads all data for the form
        /// </summary>
        private async Task LoadAllDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                statusLabel.Text = "Loading data...";

                await LoadUsersAsync();
                await LoadVehiclesAsync();
                await LoadPassengersAsync();
                await LoadRoutesForDateAsync(datePicker.Value);
                await LoadDestinationAsync();
                await LoadSchedulingSettingsAsync();
                await RefreshHistoryListView();

                statusLabel.Text = "Data loaded successfully.";
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error loading data: {ex.Message}";
                DialogHelper.ShowError($"Error loading data: {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Loads user data
        /// </summary>
        private async Task LoadUsersAsync()
        {
            try
            {
                // Get all users
                var users = await _userService.GetAllUsersAsync();

                // Set up grid columns
                usersGridView.DataSource = null;
                usersGridView.Columns.Clear();

                // Create data table
                var table = new System.Data.DataTable();
                table.Columns.Add("ID", typeof(int));
                table.Columns.Add("Username", typeof(string));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Type", typeof(string));
                table.Columns.Add("Email", typeof(string));
                table.Columns.Add("Phone", typeof(string));

                // Fill data
                foreach (var user in users)
                {
                    table.Rows.Add(user.Id, user.Username, user.Name, user.UserType, user.Email, user.Phone);
                }

                // Set data source
                usersGridView.DataSource = table;

                // Format columns
                usersGridView.Columns["ID"].Width = 50;
                usersGridView.Columns["Username"].Width = 100;
                usersGridView.Columns["Type"].Width = 80;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error loading users: {ex.Message}";
            }
        }

        /// <summary>
        /// Loads vehicle data
        /// </summary>
        private async Task LoadVehiclesAsync()
        {
            try
            {
                // Get all vehicles
                var vehicles = await _vehicleService.GetAllVehiclesAsync();

                // Set up grid columns
                driversGridView.DataSource = null;
                driversGridView.Columns.Clear();

                // Create data table
                var table = new System.Data.DataTable();
                table.Columns.Add("ID", typeof(int));
                table.Columns.Add("DriverName", typeof(string));
                table.Columns.Add("Capacity", typeof(int));
                table.Columns.Add("Address", typeof(string));
                table.Columns.Add("Available", typeof(bool));
                table.Columns.Add("Latitude", typeof(double));
                table.Columns.Add("Longitude", typeof(double));

                // Fill data
                foreach (var vehicle in vehicles)
                {
                    table.Rows.Add(
                        vehicle.Id,
                        vehicle.DriverName,
                        vehicle.Capacity,
                        vehicle.Address,
                        vehicle.IsAvailable,
                        vehicle.Latitude,
                        vehicle.Longitude
                    );
                }

                // Set data source
                driversGridView.DataSource = table;

                // Format and hide some columns
                driversGridView.Columns["ID"].Width = 50;
                driversGridView.Columns["Latitude"].Visible = false;
                driversGridView.Columns["Longitude"].Visible = false;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error loading vehicles: {ex.Message}";
            }
        }

        /// <summary>
        /// Loads passenger data
        /// </summary>
        private async Task LoadPassengersAsync()
        {
            try
            {
                // Get all passengers
                var passengers = await _passengerService.GetAllPassengersAsync();

                // Set up grid columns
                passengersGridView.DataSource = null;
                passengersGridView.Columns.Clear();

                // Create data table
                var table = new System.Data.DataTable();
                table.Columns.Add("ID", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Address", typeof(string));
                table.Columns.Add("Available", typeof(bool));
                table.Columns.Add("Latitude", typeof(double));
                table.Columns.Add("Longitude", typeof(double));

                // Fill data
                foreach (var passenger in passengers)
                {
                    table.Rows.Add(
                        passenger.Id,
                        passenger.Name,
                        passenger.Address,
                        passenger.IsAvailable,
                        passenger.Latitude,
                        passenger.Longitude
                    );
                }

                // Set data source
                passengersGridView.DataSource = table;

                // Format and hide some columns
                passengersGridView.Columns["ID"].Width = 50;
                passengersGridView.Columns["Latitude"].Visible = false;
                passengersGridView.Columns["Longitude"].Visible = false;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error loading passengers: {ex.Message}";
            }
        }

        /// <summary>
        /// Loads routes for a specific date
        /// </summary>
        private async Task LoadRoutesForDateAsync(DateTime date)
        {
            try
            {
                // Format date
                string dateStr = date.ToString("yyyy-MM-dd");
                _currentDate = dateStr;

                // Clear route display
                routeDetailsControl.Clear();
                mapControl.ClearOverlays();

                // Get routes for the date
                _currentSolution = await _routeService.GetRoutesForDateAsync(dateStr);

                // Set up grid columns
                routesGridView.DataSource = null;
                routesGridView.Columns.Clear();

                // Create data table
                var table = new System.Data.DataTable();
                table.Columns.Add("VehicleID", typeof(int));
                table.Columns.Add("Driver", typeof(string));
                table.Columns.Add("Passengers", typeof(int));
                table.Columns.Add("TotalDistance", typeof(double));
                table.Columns.Add("TotalTime", typeof(double));

                // If solution exists, fill data
                if (_currentSolution != null && _currentSolution.Vehicles.Any())
                {
                    foreach (var vehicle in _currentSolution.Vehicles.Where(v => v.Passengers.Count > 0))
                    {
                        table.Rows.Add(
                            vehicle.Id,
                            vehicle.DriverName,
                            vehicle.Passengers.Count,
                            Math.Round(vehicle.TotalDistance, 2),
                            Math.Round(vehicle.TotalTime, 0)
                        );
                    }

                    // Display the solution on the map
                    _routingService.DisplaySolutionOnMap(mapControl, _currentSolution);
                }

                // Set data source
                routesGridView.DataSource = table;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error loading routes: {ex.Message}";
            }
        }

        /// <summary>
        /// Loads destination information
        /// </summary>
        private async Task LoadDestinationAsync()
        {
            try
            {
                // Get destination info
                var destination = await _settingsRepository.GetDestinationAsync();

                // Update UI
                txtDestinationName.Text = destination.Name;
                txtDestinationAddress.Text = destination.Address;
                txtDestinationTargetTime.Text = destination.TargetTime;

                _destinationLatitude = destination.Latitude;
                _destinationLongitude = destination.Longitude;
                lblDestinationCoordinates.Text = $"{_destinationLatitude:F6}, {_destinationLongitude:F6}";

                // Show on map
                mapControl.ClearOverlays();
                mapControl.SetPosition(_destinationLatitude, _destinationLongitude, 14);
                mapControl.AddMarker(_destinationLatitude, _destinationLongitude, MarkerType.Destination, destination.Name);
                mapControl.Refresh();

                statusLabel.Text = "Destination loaded successfully.";
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error loading destination: {ex.Message}";
            }
        }

        /// <summary>
        /// Loads scheduling settings
        /// </summary>
        private async Task LoadSchedulingSettingsAsync()
        {
            try
            {
                // Get scheduling settings
                var settings = await _schedulerService.GetSchedulingSettingsAsync();

                // Update UI
                chkSchedulingEnabled.Checked = settings.IsEnabled;
                timeScheduled.Value = settings.ScheduledTime;

                statusLabel.Text = "Scheduling settings loaded successfully.";
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error loading scheduling settings: {ex.Message}";
            }
        }

        /// <summary>
        /// Refreshes the scheduling history list view
        /// </summary>
        private async Task RefreshHistoryListView()
        {
            try
            {
                // Clear list
                schedulingLogListBox.Items.Clear();

                // Get scheduling log
                var logEntries = await _schedulerService.GetSchedulingLogAsync();

                // Add entries to list box
                foreach (var entry in logEntries)
                {
                    string statusText = entry.Status;
                    string formattedEntry = $"{entry.FormattedDateTime()} - {statusText} - " +
                        $"Routes: {entry.RoutesGenerated}, Passengers: {entry.PassengersAssigned}";

                    schedulingLogListBox.Items.Add(formattedEntry);
                }

                if (schedulingLogListBox.Items.Count > 0)
                {
                    schedulingLogListBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error refreshing history: {ex.Message}";
            }
        }

        #endregion

        #region User Management

        /// <summary>
        /// Shows a dialog for adding a new user
        /// </summary>
        private void ShowAddUserDialog()
        {
            // Create a form for adding a user
            Form addUserForm = new Form
            {
                Text = "Add User",
                Width = 400,
                Height = 350,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // Create controls
            Label lblUsername = new Label { Text = "Username:", Location = new Point(20, 20), Width = 100 };
            TextBox txtUsername = new TextBox { Location = new Point(130, 20), Width = 230 };

            Label lblPassword = new Label { Text = "Password:", Location = new Point(20, 50), Width = 100 };
            TextBox txtPassword = new TextBox { Location = new Point(130, 50), Width = 230, PasswordChar = '*' };

            Label lblUserType = new Label { Text = "User Type:", Location = new Point(20, 80), Width = 100 };
            ComboBox cmbUserType = new ComboBox
            {
                Location = new Point(130, 80),
                Width = 230,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbUserType.Items.AddRange(new string[] { "Admin", "Driver", "Passenger" });
            cmbUserType.SelectedIndex = 2; // Default to Passenger

            Label lblName = new Label { Text = "Name:", Location = new Point(20, 110), Width = 100 };
            TextBox txtName = new TextBox { Location = new Point(130, 110), Width = 230 };

            Label lblEmail = new Label { Text = "Email:", Location = new Point(20, 140), Width = 100 };
            TextBox txtEmail = new TextBox { Location = new Point(130, 140), Width = 230 };

            Label lblPhone = new Label { Text = "Phone:", Location = new Point(20, 170), Width = 100 };
            TextBox txtPhone = new TextBox { Location = new Point(130, 170), Width = 230 };

            Button btnSave = new Button { Text = "Save", Location = new Point(130, 230), Width = 80 };
            Button btnCancel = new Button { Text = "Cancel", Location = new Point(220, 230), Width = 80 };

            // Add event handlers
            btnSave.Click += async (s, e) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text) ||
                        string.IsNullOrEmpty(txtName.Text))
                    {
                        MessageBox.Show("Username, password, and name are required.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Create user object
                    User newUser = new User
                    {
                        Username = txtUsername.Text.Trim(),
                        UserType = cmbUserType.SelectedItem.ToString(),
                        Name = txtName.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Phone = txtPhone.Text.Trim()
                    };

                    // Create user
                    int userId = await _userService.CreateUserAsync(newUser, txtPassword.Text);

                    if (userId > 0)
                    {
                        // If user is a driver, create a vehicle
                        if (newUser.UserType == User.DriverType)
                        {
                            await CreateDefaultVehicleForDriverAsync(userId, newUser.Name);
                        }
                        // If user is a passenger, create a passenger profile
                        else if (newUser.UserType == User.PassengerType)
                        {
                            await CreateDefaultPassengerForUserAsync(userId, newUser.Name);
                        }

                        // Refresh users
                        await LoadUsersAsync();

                        // Close dialog
                        addUserForm.DialogResult = DialogResult.OK;
                        addUserForm.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating user: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, e) =>
            {
                addUserForm.DialogResult = DialogResult.Cancel;
                addUserForm.Close();
            };

            // Add controls to form
            addUserForm.Controls.AddRange(new Control[]
            {
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                lblUserType, cmbUserType,
                lblName, txtName,
                lblEmail, txtEmail,
                lblPhone, txtPhone,
                btnSave, btnCancel
            });

            // Show dialog
            addUserForm.ShowDialog(this);
        }

        /// <summary>
        /// Edits the selected user
        /// </summary>
        private async Task EditSelectedUser()
        {
            if (usersGridView.SelectedRows.Count == 0)
                return;

            try
            {
                // Get selected user ID
                int userId = Convert.ToInt32(usersGridView.SelectedRows[0].Cells["ID"].Value);

                // Get user details
                User user = await _userService.GetUserByIdAsync(userId);

                if (user != null)
                {
                    // Create a form for editing user
                    Form editUserForm = new Form
                    {
                        Text = "Edit User",
                        Width = 400,
                        Height = 350,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        StartPosition = FormStartPosition.CenterParent,
                        MaximizeBox = false,
                        MinimizeBox = false
                    };

                    // Create controls
                    Label lblUsername = new Label { Text = "Username:", Location = new Point(20, 20), Width = 100 };
                    TextBox txtUsername = new TextBox { Location = new Point(130, 20), Width = 230, Text = user.Username, Enabled = false };

                    Label lblUserType = new Label { Text = "User Type:", Location = new Point(20, 50), Width = 100 };
                    ComboBox cmbUserType = new ComboBox
                    {
                        Location = new Point(130, 50),
                        Width = 230,
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    cmbUserType.Items.AddRange(new string[] { "Admin", "Driver", "Passenger" });
                    cmbUserType.SelectedItem = user.UserType;

                    Label lblName = new Label { Text = "Name:", Location = new Point(20, 80), Width = 100 };
                    TextBox txtName = new TextBox { Location = new Point(130, 80), Width = 230, Text = user.Name };

                    Label lblEmail = new Label { Text = "Email:", Location = new Point(20, 110), Width = 100 };
                    TextBox txtEmail = new TextBox { Location = new Point(130, 110), Width = 230, Text = user.Email };

                    Label lblPhone = new Label { Text = "Phone:", Location = new Point(20, 140), Width = 100 };
                    TextBox txtPhone = new TextBox { Location = new Point(130, 140), Width = 230, Text = user.Phone };

                    Label lblNewPassword = new Label { Text = "New Password:", Location = new Point(20, 170), Width = 100 };
                    TextBox txtNewPassword = new TextBox { Location = new Point(130, 170), Width = 230, PasswordChar = '*' };

                    Button btnSave = new Button { Text = "Save", Location = new Point(130, 230), Width = 80 };
                    Button btnCancel = new Button { Text = "Cancel", Location = new Point(220, 230), Width = 80 };

                    // Add event handlers
                    btnSave.Click += async (s, e) =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(txtName.Text))
                            {
                                MessageBox.Show("Name cannot be empty.", "Validation Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Update user object
                            user.UserType = cmbUserType.SelectedItem.ToString();
                            user.Name = txtName.Text.Trim();
                            user.Email = txtEmail.Text.Trim();
                            user.Phone = txtPhone.Text.Trim();

                            // Update user
                            bool success = await _userService.UpdateUserAsync(user);

                            // Change password if provided
                            if (!string.IsNullOrEmpty(txtNewPassword.Text))
                            {
                                await _userService.ChangePasswordAsync(userId, txtNewPassword.Text);
                            }

                            if (success)
                            {
                                // Refresh users
                                await LoadUsersAsync();

                                // Close dialog
                                editUserForm.DialogResult = DialogResult.OK;
                                editUserForm.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error updating user: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    btnCancel.Click += (s, e) =>
                    {
                        editUserForm.DialogResult = DialogResult.Cancel;
                        editUserForm.Close();
                    };

                    // Add controls to form
                    editUserForm.Controls.AddRange(new Control[]
                    {
                        lblUsername, txtUsername,
                        lblUserType, cmbUserType,
                        lblName, txtName,
                        lblEmail, txtEmail,
                        lblPhone, txtPhone,
                        lblNewPassword, txtNewPassword,
                        btnSave, btnCancel
                    });

                    // Show dialog
                    editUserForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error editing user: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes the selected user
        /// </summary>
        private async Task DeleteSelectedUser()
        {
            if (usersGridView.SelectedRows.Count == 0)
                return;

            try
            {
                // Get selected user ID
                int userId = Convert.ToInt32(usersGridView.SelectedRows[0].Cells["ID"].Value);
                string username = usersGridView.SelectedRows[0].Cells["Username"].Value.ToString();

                // Confirm deletion
                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to delete user '{username}'?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // Delete user
                    bool success = await _userService.DeleteUserAsync(userId);

                    if (success)
                    {
                        // Refresh users
                        await LoadUsersAsync();
                        statusLabel.Text = $"User '{username}' deleted successfully.";
                    }
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error deleting user: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a default vehicle for a driver
        /// </summary>
        private async Task CreateDefaultVehicleForDriverAsync(int userId, string driverName)
        {
            try
            {
                // Get destination for default location
                var destination = await _settingsRepository.GetDestinationAsync();

                // Create default vehicle
                Vehicle vehicle = new Vehicle
                {
                    UserId = userId,
                    DriverName = driverName,
                    Capacity = 4,
                    Latitude = destination.Latitude - 0.01, // Slightly offset from destination
                    Longitude = destination.Longitude - 0.01,
                    Address = "Default Location",
                    IsAvailable = false
                };

                // Create vehicle
                await _vehicleService.CreateVehicleAsync(vehicle);
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error creating default vehicle: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a default passenger profile for a user
        /// </summary>
        private async Task CreateDefaultPassengerForUserAsync(int userId, string name)
        {
            try
            {
                // Get destination for default location
                var destination = await _settingsRepository.GetDestinationAsync();

                // Create default passenger
                Passenger passenger = new Passenger
                {
                    UserId = userId,
                    Name = name,
                    Latitude = destination.Latitude - 0.02, // Slightly offset from destination
                    Longitude = destination.Longitude - 0.02,
                    Address = "Default Location",
                    IsAvailable = false
                };

                // Create passenger
                await _passengerService.CreatePassengerAsync(passenger);
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Error creating default passenger: {ex.Message}");
            }
        }

        #endregion

        #region Map and Display Methods

        /// <summary>
        /// Shows the selected driver on the map
        /// </summary>
        private void ShowSelectedDriverOnMap()
        {
            if (driversGridView.SelectedRows.Count == 0)
                return;

            try
            {
                // Get selected vehicle data
                double latitude = Convert.ToDouble(driversGridView.SelectedRows[0].Cells["Latitude"].Value);
                double longitude = Convert.ToDouble(driversGridView.SelectedRows[0].Cells["Longitude"].Value);
                string driverName = driversGridView.SelectedRows[0].Cells["DriverName"].Value.ToString();
                int capacity = Convert.ToInt32(driversGridView.SelectedRows[0].Cells["Capacity"].Value);

                // Clear map
                mapControl.ClearOverlays();

                // Add marker
                mapControl.AddMarker(latitude, longitude, MarkerType.Vehicle,
                    $"Driver: {driverName} (Capacity: {capacity})");

                // Center map
                mapControl.SetPosition(latitude, longitude, 14);

                // Refresh map
                mapControl.Refresh();
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error showing driver on map: {ex.Message}";
            }
        }

        /// <summary>
        /// Shows the selected passenger on the map
        /// </summary>
        private void ShowSelectedPassengerOnMap()
        {
            if (passengersGridView.SelectedRows.Count == 0)
                return;

            try
            {
                // Get selected passenger data
                double latitude = Convert.ToDouble(passengersGridView.SelectedRows[0].Cells["Latitude"].Value);
                double longitude = Convert.ToDouble(passengersGridView.SelectedRows[0].Cells["Longitude"].Value);
                string name = passengersGridView.SelectedRows[0].Cells["Name"].Value.ToString();

                // Clear map
                mapControl.ClearOverlays();

                // Add marker
                mapControl.AddMarker(latitude, longitude, MarkerType.Passenger,
                    $"Passenger: {name}");

                // Center map
                mapControl.SetPosition(latitude, longitude, 14);

                // Refresh map
                mapControl.Refresh();
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error showing passenger on map: {ex.Message}";
            }
        }

        /// <summary>
        /// Displays the selected route
        /// </summary>
        private async Task DisplaySelectedRoute()
        {
            if (routesGridView.SelectedRows.Count == 0 || _currentSolution == null)
                return;

            try
            {
                // Get selected vehicle ID
                int vehicleId = Convert.ToInt32(routesGridView.SelectedRows[0].Cells["VehicleID"].Value);

                // Find the vehicle in the solution
                var vehicle = _currentSolution.Vehicles.FirstOrDefault(v => v.Id == vehicleId);

                if (vehicle != null)
                {
                    // Get route details
                    var routeDetails = await _routeService.GetRouteDetailsAsync(vehicleId, _currentDate);

                    if (routeDetails != null)
                    {
                        // Display route details
                        routeDetailsControl.DisplayRouteDetails(vehicle, routeDetails);

                        // Center map on vehicle
                        mapControl.SetPosition(vehicle.Latitude, vehicle.Longitude, 13);

                        // Highlight route on map
                        _routingService.DisplaySolutionOnMap(mapControl, new Solution
                        {
                            Vehicles = new List<Vehicle> { vehicle }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error displaying route: {ex.Message}";
            }
        }

        /// <summary>
        /// Edits the selected passenger
        /// </summary>
        private void EditSelectedPassenger()
        {
            if (passengersGridView.SelectedRows.Count == 0)
                return;

            try
            {
                // Get selected passenger ID
                int passengerId = Convert.ToInt32(passengersGridView.SelectedRows[0].Cells["ID"].Value);
                string name = passengersGridView.SelectedRows[0].Cells["Name"].Value.ToString();

                // Show dialog to edit passenger (implementation omitted for brevity)
                DialogHelper.ShowInfo($"Edit function for passenger {name} would be implemented here.");
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error editing passenger: {ex.Message}";
            }
        }

        #endregion

        #region Destination and Scheduling Methods

        /// <summary>
        /// Enables destination location selection on the map
        /// </summary>
        private void EnableDestinationLocationSelection()
        {
            // Enable location selection mode
            _locationSelectionMode = true;

            // Change button text
            btnSetDestinationOnMap.Text = "Click on map to set...";
            btnSetDestinationOnMap.BackColor = Color.LightBlue;

            // Show instructions
            DialogHelper.ShowInfo("Click on the map to set the destination location.");

            // Subscribe to map click event
            mapControl.MapClick += DestinationMapClick;
        }

        /// <summary>
        /// Handles map click for destination location selection
        /// </summary>
        private void DestinationMapClick(object sender, MapClickEventArgs e)
        {
            if (_locationSelectionMode)
            {
                // Disable location selection mode
                _locationSelectionMode = false;

                // Reset button
                btnSetDestinationOnMap.Text = "Set Location on Map";
                btnSetDestinationOnMap.BackColor = SystemColors.Control;

                // Unsubscribe from event
                mapControl.MapClick -= DestinationMapClick;

                // Update coordinates
                _destinationLatitude = e.Latitude;
                _destinationLongitude = e.Longitude;
                lblDestinationCoordinates.Text = $"{_destinationLatitude:F6}, {_destinationLongitude:F6}";

                // Update map
                mapControl.ClearOverlays();
                mapControl.AddMarker(_destinationLatitude, _destinationLongitude, MarkerType.Destination, txtDestinationName.Text);

                // Try to get address
                GetAddressForDestination(_destinationLatitude, _destinationLongitude);
            }
        }

        /// <summary>
        /// Gets address for the destination coordinates
        /// </summary>
        private async void GetAddressForDestination(double latitude, double longitude)
        {
            try
            {
                string address = await _mapService.ReverseGeocodeAsync(latitude, longitude);

                if (!string.IsNullOrEmpty(address))
                {
                    txtDestinationAddress.Text = address;
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error getting address: {ex.Message}";
            }
        }

        /// <summary>
        /// Updates the destination
        /// </summary>
        private async Task UpdateDestinationAsync()
        {
            try
            {
                // Validate
                if (string.IsNullOrEmpty(txtDestinationName.Text) ||
                    string.IsNullOrEmpty(txtDestinationAddress.Text) ||
                    string.IsNullOrEmpty(txtDestinationTargetTime.Text))
                {
                    DialogHelper.ShowWarning("All fields are required.");
                    return;
                }

                // Validate target time format (HH:MM)
                if (!System.Text.RegularExpressions.Regex.IsMatch(txtDestinationTargetTime.Text, @"^([01]?[0-9]|2[0-3]):[0-5][0-9]$"))
                {
                    DialogHelper.ShowWarning("Target time must be in format HH:MM");
                    return;
                }

                // Update destination
                bool success = await _settingsRepository.UpdateDestinationAsync(
                    txtDestinationName.Text,
                    _destinationLatitude,
                    _destinationLongitude,
                    txtDestinationTargetTime.Text,
                    txtDestinationAddress.Text);

                if (success)
                {
                    statusLabel.Text = "Destination updated successfully.";

                    // Refresh map
                    mapControl.ClearOverlays();
                    mapControl.AddMarker(_destinationLatitude, _destinationLongitude, MarkerType.Destination, txtDestinationName.Text);
                }
                else
                {
                    statusLabel.Text = "Failed to update destination.";
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error updating destination: {ex.Message}";
            }
        }

        /// <summary>
        /// Saves scheduling settings
        /// </summary>
        private async Task SaveSchedulingSettingsAsync()
        {
            try
            {
                // Save settings
                bool success = await _schedulerService.SaveSchedulingSettingsAsync(
                    chkSchedulingEnabled.Checked,
                    timeScheduled.Value);

                if (success)
                {
                    statusLabel.Text = "Scheduling settings saved successfully.";
                }
                else
                {
                    statusLabel.Text = "Failed to save scheduling settings.";
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error saving settings: {ex.Message}";
            }
        }

        /// <summary>
        /// Runs the scheduler
        /// </summary>
        private async Task RunSchedulerAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnRunNow.Enabled = false;
                statusLabel.Text = "Running scheduler...";

                // Run scheduler
                bool success = await _schedulerService.RunSchedulerAsync();

                if (success)
                {
                    statusLabel.Text = "Scheduler ran successfully.";

                    // Refresh history
                    await RefreshHistoryListView();

                    // Load routes for today
                    await LoadRoutesForDateAsync(DateTime.Today);
                }
                else
                {
                    statusLabel.Text = "Scheduler failed to run.";
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error running scheduler: {ex.Message}";
            }
            finally
            {
                btnRunNow.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Generates routes for the current date
        /// </summary>
        private async Task GenerateRoutesAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                statusLabel.Text = "Generating routes...";

                // Get available vehicles and passengers
                var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
                var passengers = await _passengerService.GetAvailablePassengersAsync();

                if (!vehicles.Any() || !passengers.Any())
                {
                    DialogHelper.ShowWarning("Need at least one available vehicle and passenger.");
                    return;
                }

                // Generate routes
                var solution = await _routeService.GenerateRoutesAsync(
                    _currentDate,
                    vehicles,
                    passengers);

                if (solution != null)
                {
                    // Save routes
                    int routeId = await _routeService.SaveRoutesAsync(solution, _currentDate);

                    if (routeId > 0)
                    {
                        statusLabel.Text = "Routes generated successfully.";

                        // Reload routes
                        await LoadRoutesForDateAsync(datePicker.Value);
                    }
                    else
                    {
                        statusLabel.Text = "Failed to save routes.";
                    }
                }
                else
                {
                    statusLabel.Text = "Failed to generate routes.";
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error generating routes: {ex.Message}";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        /// <summary>
        /// Handles logout
        /// </summary>
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