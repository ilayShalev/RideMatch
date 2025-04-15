using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.Repository;
using RideMatch.UI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RideMatch.UI.Helpers
{
    public static class ControlFactory
    {
        // Services for dependency injection
        private static IUserService _userService;
        private static IVehicleService _vehicleService;
        private static IPassengerService _passengerService;
        private static IMapService _mapService;
        private static IRouteService _routeService;
        private static IRoutingService _routingService;
        private static ISchedulerService _schedulerService;
        private static SettingsRepository _settingsRepository;

        // Initialize services - would be called at application startup
        public static void InitializeServices(
            IUserService userService,
            IVehicleService vehicleService,
            IPassengerService passengerService,
            IMapService mapService,
            IRouteService routeService,
            IRoutingService routingService,
            ISchedulerService schedulerService,
            SettingsRepository settingsRepository)
        {
            _userService = userService;
            _vehicleService = vehicleService;
            _passengerService = passengerService;
            _mapService = mapService;
            _routeService = routeService;
            _routingService = routingService;
            _schedulerService = schedulerService;
            _settingsRepository = settingsRepository;
        }

        // Creates a label control
        public static Label CreateLabel(string text, Point location, Size size, Font font = null, ContentAlignment alignment = ContentAlignment.MiddleLeft)
        {
            return new Label
            {
                Text = text,
                Location = location,
                Size = size,
                Font = font ?? SystemFonts.DefaultFont,
                TextAlign = alignment
            };
        }

        // Creates a button control
        public static Button CreateButton(string text, Point location, Size size, EventHandler onClick = null)
        {
            Button button = new Button
            {
                Text = text,
                Location = location,
                Size = size
            };

            if (onClick != null)
            {
                button.Click += onClick;
            }

            return button;
        }

        // Creates a text box control
        public static TextBox CreateTextBox(Point location, Size size, string text = "", bool multiline = false, bool readOnly = false)
        {
            return new TextBox
            {
                Location = location,
                Size = size,
                Text = text,
                Multiline = multiline,
                ReadOnly = readOnly
            };
        }

        // Creates a rich text box
        public static RichTextBox CreateRichTextBox(Point location, Size size, bool readOnly = false)
        {
            return new RichTextBox
            {
                Location = location,
                Size = size,
                ReadOnly = readOnly
            };
        }

        // Creates a numeric up/down control
        public static NumericUpDown CreateNumericUpDown(Point location, Size size, decimal min, decimal max, decimal value, decimal increment = 1)
        {
            return new NumericUpDown
            {
                Location = location,
                Size = size,
                Minimum = min,
                Maximum = max,
                Value = value,
                Increment = increment
            };
        }

        // Creates a combo box
        public static ComboBox CreateComboBox(Point location, Size size, string[] items, int selectedIndex = 0)
        {
            ComboBox comboBox = new ComboBox
            {
                Location = location,
                Size = size,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            if (items != null)
            {
                comboBox.Items.AddRange(items);
                if (selectedIndex >= 0 && selectedIndex < items.Length)
                {
                    comboBox.SelectedIndex = selectedIndex;
                }
            }

            return comboBox;
        }

        // Creates a check box
        public static CheckBox CreateCheckBox(string text, Point location, Size size, bool isChecked = false)
        {
            return new CheckBox
            {
                Text = text,
                Location = location,
                Size = size,
                Checked = isChecked
            };
        }

        // Creates a radio button
        public static RadioButton CreateRadioButton(string text, Point location, Size size, bool isChecked = false)
        {
            return new RadioButton
            {
                Text = text,
                Location = location,
                Size = size,
                Checked = isChecked
            };
        }

        // Creates a panel
        public static Panel CreatePanel(Point location, Size size, BorderStyle borderStyle = BorderStyle.None)
        {
            return new Panel
            {
                Location = location,
                Size = size,
                BorderStyle = borderStyle
            };
        }

        // Appends a log message to a text box
        public static void AppendLog(this TextBox textBox, string message)
        {
            if (textBox == null)
                return;

            // Append message with timestamp
            string logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";

            // Append to text box
            textBox.AppendText(logMessage);

            // Scroll to end
            textBox.SelectionStart = textBox.Text.Length;
            textBox.ScrollToCaret();
        }

        // Creates the admin form
        public static Form CreateAdminForm(User user)
        {
            CheckServices();
            return new AdminForm(user, _userService, _vehicleService, _passengerService,
                                _mapService, _routeService, _routingService,
                                _schedulerService, _settingsRepository);
        }

        // Creates the driver form
        public static Form CreateDriverForm(User user)
        {
            CheckServices();
            return new DriverForm(user, _vehicleService, _mapService, _routeService);
        }

        // Creates the passenger form
        public static Form CreatePassengerForm(User user)
        {
            CheckServices();
            return new PassengerForm(user, _passengerService, _mapService, _routeService);
        }

        // Creates the registration form
        public static IRegistrationForm CreateRegistrationForm(IUserService userService)
        {
            // Return the real implementation
            return new RegistrationForm(userService);
        }

        // Check if services are initialized
        private static void CheckServices()
        {
            if (_userService == null)
            {
                throw new InvalidOperationException("Services have not been initialized. Call InitializeServices first.");
            }
        }
    }

    // Interface for registration form
    public interface IRegistrationForm
    {
        string Username { get; }
        DialogResult ShowDialog(IWin32Window owner);
    }
}