using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Controls
{
    /// <summary>
    /// A control for searching and selecting addresses with geocoding capability
    /// </summary>
    public partial class AddressSearchControl : UserControl
    {
        private TextBox txtAddress;
        private Button btnSearch;
        private Label lblStatus;
        private ComboBox cmbSuggestions;
        private Timer suggestionTimer;

        private IMapService _mapService;

        // Event for when an address is found
        public event EventHandler<AddressFoundEventArgs> AddressFound;

        /// <summary>
        /// Initializes a new instance of the AddressSearchControl
        /// </summary>
        public AddressSearchControl()
        {
            InitializeComponent();

   
        }



        /// <summary>
        /// Handles the Load event
        /// </summary>
        private void AddressSearchControl_Load(object sender, EventArgs e)
        {
            // Try to get map service from parent form
            if (_mapService == null)
            {
                TryGetMapServiceFromParent();
            }
        }


        private void RouteDetailsControl_Load(object sender, EventArgs e)
        {
            // Empty implementation - keeps the designer happy
        }

        /// <summary>
        /// Tries to get the map service from the parent form
        /// </summary>
        private void TryGetMapServiceFromParent()
        {
            try
            {
                // Look for the form that contains this control
                Form parentForm = FindForm();
                if (parentForm != null)
                {
                    // Try to get the map service using reflection
                    var fields = parentForm.GetType().GetFields(
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

                    foreach (var field in fields)
                    {
                        if (typeof(IMapService).IsAssignableFrom(field.FieldType))
                        {
                            var service = field.GetValue(parentForm) as IMapService;
                            if (service != null)
                            {
                                _mapService = service;
                                return;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore reflection errors
            }
        }

        /// <summary>
        /// Sets the map service manually
        /// </summary>
        public void SetMapService(IMapService mapService)
        {
            _mapService = mapService;
        }

        /// <summary>
        /// Handles text changes in the address textbox
        /// </summary>
        private void TxtAddress_TextChanged(object sender, EventArgs e)
        {
            // Reset and restart the timer each time text changes
            suggestionTimer.Stop();

            string query = txtAddress.Text.Trim();
            if (query.Length > 2)
            {
                suggestionTimer.Start();
            }
            else
            {
                cmbSuggestions.Visible = false;
            }
        }

        /// <summary>
        /// Gets address suggestions for the current text
        /// </summary>
        private async Task GetAddressSuggestionsAsync()
        {
            if (_mapService == null)
                return;

            string query = txtAddress.Text.Trim();
            if (query.Length > 2)
            {
                try
                {
                    // Get address suggestions
                    var suggestions = await _mapService.GetAddressSuggestionsAsync(query);

                    // Update suggestions dropdown
                    cmbSuggestions.Items.Clear();

                    if (suggestions.Count > 0)
                    {
                        cmbSuggestions.Items.AddRange(suggestions.ToArray());
                        cmbSuggestions.Visible = true;
                        cmbSuggestions.DroppedDown = true;
                    }
                    else
                    {
                        cmbSuggestions.Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    // Just ignore errors during suggestions
                    cmbSuggestions.Visible = false;
                    Console.WriteLine($"Error getting suggestions: {ex.Message}");
                }
            }
            else
            {
                cmbSuggestions.Visible = false;
            }
        }

        /// <summary>
        /// Handles key presses in the address textbox
        /// </summary>
        private void TxtAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                // Trigger search
                _ = SearchAddressAsync();
            }
            else if (e.KeyCode == Keys.Down && cmbSuggestions.Visible)
            {
                // Move focus to suggestions dropdown
                cmbSuggestions.Focus();
                if (cmbSuggestions.Items.Count > 0 && cmbSuggestions.SelectedIndex < 0)
                {
                    cmbSuggestions.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Handles selection changes in the suggestions dropdown
        /// </summary>
        private void CmbSuggestions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSuggestions.SelectedItem != null)
            {
                txtAddress.Text = cmbSuggestions.SelectedItem.ToString();
                cmbSuggestions.Visible = false;

                // Trigger search
                _ = SearchAddressAsync();
            }
        }

        /// <summary>
        /// Searches for an address
        /// </summary>
        public async Task SearchAddressAsync()
        {
            if (_mapService == null)
            {
                ShowStatus("Map service not available", false);
                return;
            }

            string address = txtAddress.Text.Trim();
            if (string.IsNullOrEmpty(address))
            {
                ShowStatus("Please enter an address", false);
                return;
            }

            try
            {
                // Show searching status
                ShowStatus("Searching...", true);

                // Get coordinates from address
                var coordinates = await _mapService.GeocodeAddressAsync(address);

                if (coordinates.HasValue)
                {
                    // Success
                    ShowStatus("Address found", true);

                    // Raise event
                    OnAddressFound(new AddressFoundEventArgs
                    {
                        Address = address,
                        Latitude = coordinates.Value.Latitude,
                        Longitude = coordinates.Value.Longitude
                    });
                }
                else
                {
                    // Address not found
                    ShowStatus("Address not found", false);
                }
            }
            catch (Exception ex)
            {
                // Error
                ShowStatus($"Error: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Shows a status message
        /// </summary>
        private void ShowStatus(string message, bool success)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = success ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Raises the AddressFound event
        /// </summary>
        protected virtual void OnAddressFound(AddressFoundEventArgs e)
        {
            AddressFound?.Invoke(this, e);
        }

        private void AddressSearchControl_Load_1(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// Event args for address found event
    /// </summary>
    public class AddressFoundEventArgs : EventArgs
    {
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }




}