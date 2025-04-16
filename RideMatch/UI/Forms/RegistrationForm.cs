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