using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Helpers;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    /// <summary>
    /// Registration form for new users
    /// </summary>
    public partial class RegistrationForm : Form, IRegistrationForm
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private ComboBox cmbUserType;
        private TextBox txtName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private Button btnRegister;
        private Button btnCancel;
        private Label lblStatus;

        private readonly IUserService _userService;

        /// <summary>
        /// Gets the username of the registered user
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Initializes a new instance of the RegistrationForm
        /// </summary>
        /// <param name="userService">The user service</param>
        public RegistrationForm(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the form components
        /// </summary>
        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Register New User";
            this.Size = new Size(400, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AcceptButton = btnRegister;
            this.CancelButton = btnCancel;

            // Title label
            Label lblTitle = new Label
            {
                Text = "Create a New Account",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Username
            Label lblUsername = new Label { Text = "Username:", Location = new Point(20, 70), Width = 100 };
            txtUsername = new TextBox { Location = new Point(130, 70), Width = 230 };
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);

            // Password
            Label lblPassword = new Label { Text = "Password:", Location = new Point(20, 100), Width = 100 };
            txtPassword = new TextBox { Location = new Point(130, 100), Width = 230, PasswordChar = '*' };
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);

            // Confirm Password
            Label lblConfirmPassword = new Label { Text = "Confirm Password:", Location = new Point(20, 130), Width = 110 };
            txtConfirmPassword = new TextBox { Location = new Point(130, 130), Width = 230, PasswordChar = '*' };
            this.Controls.Add(lblConfirmPassword);
            this.Controls.Add(txtConfirmPassword);

            // User Type
            Label lblUserType = new Label { Text = "Register as:", Location = new Point(20, 160), Width = 100 };
            cmbUserType = new ComboBox
            {
                Location = new Point(130, 160),
                Width = 230,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbUserType.Items.AddRange(new string[] { User.DriverType, User.PassengerType });
            cmbUserType.SelectedIndex = 1; // Default to Passenger
            this.Controls.Add(lblUserType);
            this.Controls.Add(cmbUserType);

            // Name
            Label lblName = new Label { Text = "Full Name:", Location = new Point(20, 190), Width = 100 };
            txtName = new TextBox { Location = new Point(130, 190), Width = 230 };
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);

            // Email
            Label lblEmail = new Label { Text = "Email:", Location = new Point(20, 220), Width = 100 };
            txtEmail = new TextBox { Location = new Point(130, 220), Width = 230 };
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);

            // Phone
            Label lblPhone = new Label { Text = "Phone:", Location = new Point(20, 250), Width = 100 };
            txtPhone = new TextBox { Location = new Point(130, 250), Width = 230 };
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);

            // Status
            lblStatus = new Label
            {
                Location = new Point(20, 290),
                Size = new Size(340, 40),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblStatus);

            // Buttons
            btnRegister = new Button
            {
                Text = "Register",
                Location = new Point(130, 330),
                Size = new Size(100, 30)
            };
            btnRegister.Click += BtnRegister_Click;
            this.Controls.Add(btnRegister);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(240, 330),
                Size = new Size(100, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancel);
        }

        /// <summary>
        /// Handles the Register button click
        /// </summary>
        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            // Clear status
            lblStatus.Text = "";

            // Validate input
            if (!ValidateInput())
                return;

            try
            {
                // Disable controls during registration
                SetControlsEnabled(false);
                lblStatus.Text = "Registering...";
                lblStatus.ForeColor = Color.Blue;

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
                    // Store username for the login form
                    Username = newUser.Username;

                    // Successful registration
                    MessageBox.Show(
                        "Registration successful! You can now log in with your credentials.",
                        "Registration Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Close with success
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblStatus.Text = "Registration failed. Please try again.";
                    lblStatus.ForeColor = Color.Red;
                    SetControlsEnabled(true);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
                lblStatus.ForeColor = Color.Red;
                SetControlsEnabled(true);
            }
        }

        /// <summary>
        /// Validates the user input
        /// </summary>
        /// <returns>True if input is valid, otherwise false</returns>
        private bool ValidateInput()
        {
            // Check required fields
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                lblStatus.Text = "Username is required.";
                txtUsername.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblStatus.Text = "Password is required.";
                txtPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Name is required.";
                txtName.Focus();
                return false;
            }

            // Check username format (letters, numbers, and underscore)
            if (!Regex.IsMatch(txtUsername.Text, @"^[a-zA-Z0-9_]+$"))
            {
                lblStatus.Text = "Username can only contain letters, numbers, and underscores.";
                txtUsername.Focus();
                return false;
            }

            // Check password length
            if (txtPassword.Text.Length < 6)
            {
                lblStatus.Text = "Password must be at least 6 characters.";
                txtPassword.Focus();
                return false;
            }

            // Check password match
            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                lblStatus.Text = "Passwords do not match.";
                txtConfirmPassword.Focus();
                return false;
            }

            // Check email format if provided
            if (!string.IsNullOrEmpty(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                lblStatus.Text = "Invalid email format.";
                txtEmail.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates an email address
        /// </summary>
        /// <param name="email">The email to validate</param>
        /// <returns>True if email is valid, otherwise false</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                // Use regular expression to validate email
                return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enables or disables the form controls
        /// </summary>
        /// <param name="enabled">Whether controls should be enabled</param>
        private void SetControlsEnabled(bool enabled)
        {
            txtUsername.Enabled = enabled;
            txtPassword.Enabled = enabled;
            txtConfirmPassword.Enabled = enabled;
            cmbUserType.Enabled = enabled;
            txtName.Enabled = enabled;
            txtEmail.Enabled = enabled;
            txtPhone.Enabled = enabled;
            btnRegister.Enabled = enabled;
        }
    }
}