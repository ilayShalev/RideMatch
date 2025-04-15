using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Controls
{
    public class AddressSearchControl : UserControl
    {
        private TextBox txtAddress;
        private Button btnSearch;
        private Label lblStatus;
        private ComboBox cmbSuggestions;

        private IMapService _mapService;

        // Event for when an address is found
        public event EventHandler<AddressFoundEventArgs> AddressFound;

        public AddressSearchControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Setup layout
            this.Size = new Size(300, 80);

            // Address text box
            txtAddress = new TextBox
            {
                Location = new Point(0, 0),
                Size = new Size(220, 20),
                PlaceholderText = "Enter address to search"
            };
            txtAddress.TextChanged += TxtAddress_TextChanged;
            txtAddress.KeyDown += TxtAddress_KeyDown;
            this.Controls.Add(txtAddress);

            // Search button
            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(225, 0),
                Size = new Size(75, 23)
            };
            btnSearch.Click += async (sender, e) => await SearchAddressAsync();
            this.Controls.Add(btnSearch);

            // Suggestions dropdown
            cmbSuggestions = new ComboBox
            {
                Location = new Point(0, 25),
                Size = new Size(300, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };
            cmbSuggestions.SelectedIndexChanged += CmbSuggestions_SelectedIndexChanged;
            this.Controls.Add(cmbSuggestions);

            // Status label
            lblStatus = new Label
            {
                Location = new Point(0, 50),
                Size = new Size(300, 20),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblStatus);
        }

        // Initialize with map service
        private void AddressSearchControl_Load(object sender, EventArgs e)
        {
            // Get map service from dependency injection
            if (_mapService == null)
            {
                // This is a placeholder - in a real app, we would get this from DI
                // For now, let's try to get it from the containing form
                Form parentForm = FindForm();
                if (parentForm != null)
                {
                    // Try to find _mapService through reflection or other means
                    // This is just a placeholder approach
                }
            }
        }

        // Set the map service manually
        public void SetMapService(IMapService mapService)
        {
            _mapService = mapService;
        }

        private async void TxtAddress_TextChanged(object sender, EventArgs e)
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
                }
            }
            else
            {
                cmbSuggestions.Visible = false;
            }
        }

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

        private void ShowStatus(string message, bool success)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = success ? Color.Green : Color.Red;
        }

        protected virtual void OnAddressFound(AddressFoundEventArgs e)
        {
            AddressFound?.Invoke(this, e);
        }
    }

    // Event args for address found event
    public class AddressFoundEventArgs : EventArgs
    {
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}