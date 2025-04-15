using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Helpers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    /// <summary>
    /// Form for passenger users to manage their ride requests and view assignments
    /// </summary>
    public class PassengerForm : BaseUserForm
    {
        private Passenger _passenger;

        private readonly IPassengerService _passengerService;
        private readonly IMapService _mapService;
        private readonly IRouteService _routeService;

        /// <summary>
        /// Initializes a new instance of the PassengerForm class
        /// </summary>
        public PassengerForm(User user, IPassengerService passengerService, IMapService mapService, IRouteService routeService)
            : base(user)
        {
            _passengerService = passengerService ?? throw new ArgumentNullException(nameof(passengerService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));

            this.Text = "RideMatch - Passenger Dashboard";

            // Initialize map
            _mapService.InitializeMap(mapControl);

            // Initialize address search control
            addressSearchControl.SetMapService(_mapService);

            // Load data when form loads
            this.Load += async (sender, e) => await LoadDataAsync();
        }

        /// <summary>
        /// Gets the title for the info panel
        /// </summary>
        protected override string GetInfoPanelTitle() => "Ride Request";

        /// <summary>
        /// Gets the title for the details panel
        /// </summary>
        protected override string GetDetailsPanelTitle() => "Ride Assignment";

        /// <summary>
        /// Gets the title for the map controls
        /// </summary>
        protected override string GetMapControlsTitle() => "Set Your Pickup Location";

        /// <summary>
        /// Adds additional controls to the info panel
        /// </summary>
        protected override void AddInfoControls(Panel panel)
        {
            // No additional controls needed for passenger
        }

        /// <summary>
        /// Updates the passenger's availability
        /// </summary>
        protected override async Task UpdateAvailabilityAsync()
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

        /// <summary>
        /// Updates the passenger's location
        /// </summary>
        protected override async void UpdateLocationAsync(double latitude, double longitude)
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

        /// <summary>
        /// Loads passenger data
        /// </summary>
        protected override async Task LoadDataAsync()
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

        /// <summary>
        /// Displays the passenger on the map
        /// </summary>
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

        /// <summary>
        /// Updates the assignment details text
        /// </summary>
        private async void UpdateAssignmentDetailsText()
        {
            try
            {
                // Clear current text
                txtDetails.Text = string.Empty;

                if (_passenger == null)
                    return;

                // Get today's date
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // Get assignment for passenger
                var assignment = await _routeService.GetPassengerAssignmentAsync(_currentUser.Id, today);

                txtDetails.AppendText("TODAY'S RIDE ASSIGNMENT\n", Color.Blue, true);
                txtDetails.AppendText("=======================\n\n");

                if (assignment.AssignedVehicle != null)
                {
                    // Display assignment details
                    txtDetails.AppendText("Status: ", Color.Black, true);
                    txtDetails.AppendText("ASSIGNED\n\n", Color.Green, true);

                    // Driver info
                    txtDetails.AppendText("Driver: ", Color.Black, true);
                    txtDetails.AppendText($"{assignment.AssignedVehicle.DriverName}\n");

                    // Pickup time
                    txtDetails.AppendText("Pickup Time: ", Color.Black, true);
                    if (assignment.PickupTime.HasValue)
                    {
                        txtDetails.AppendText($"{assignment.PickupTime.Value.ToString("HH:mm")}\n");
                    }
                    else
                    {
                        txtDetails.AppendText("Not scheduled yet\n");
                    }

                    // Vehicle info
                    txtDetails.AppendText("\nVEHICLE DETAILS\n", Color.DarkBlue, true);
                    txtDetails.AppendText("---------------\n");
                    txtDetails.AppendText("Vehicle Capacity: ", Color.Black, false);
                    txtDetails.AppendText($"{assignment.AssignedVehicle.Capacity} passengers\n\n");

                    // Pickup location
                    txtDetails.AppendText("PICKUP LOCATION\n", Color.DarkBlue, true);
                    txtDetails.AppendText("---------------\n");
                    txtDetails.AppendText($"{_passenger.Address}\n");

                    // Instructions
                    txtDetails.AppendText("\nINSTRUCTIONS\n", Color.DarkBlue, true);
                    txtDetails.AppendText("------------\n");
                    txtDetails.AppendText("Please be at your pickup location 5 minutes before the scheduled time.\n");
                    txtDetails.AppendText("Contact the driver if you need to cancel or reschedule.\n");
                }
                else
                {
                    // No assignment
                    txtDetails.AppendText("Status: ", Color.Black, true);

                    if (_passenger.IsAvailable)
                    {
                        txtDetails.AppendText("PENDING\n\n", Color.Orange, true);
                        txtDetails.AppendText("Your ride request is pending assignment.\n");
                        txtDetails.AppendText("Please check back later or use the Refresh button.\n\n");
                        txtDetails.AppendText("Your pickup location: ", Color.Black, true);
                        txtDetails.AppendText($"{_passenger.Address}\n");
                    }
                    else
                    {
                        txtDetails.AppendText("NOT REQUESTED\n\n", Color.Red, true);
                        txtDetails.AppendText("You have not requested a ride for today.\n");
                        txtDetails.AppendText("To request a ride, check the 'Available for today's rides' box.\n");
                    }
                }
            }
            catch (Exception ex)
            {
                txtDetails.Text = $"Error loading assignment details: {ex.Message}";
            }
        }
    }
}