using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.UI.Forms;
using RideMatch.UI.Helpers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Label lblStatus;
        private Panel mainPanel;
        private PictureBox logoBox;

        private readonly IUserService _userService;

        public LoginForm(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            InitializeComponent();
        }


        private async Task LoginAsync()
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    lblStatus.Text = "Please enter both username and password.";
                    return;
                }

                // Show loading indicator
                lblStatus.ForeColor = Color.Blue;
                lblStatus.Text = "Logging in...";
                btnLogin.Enabled = false;
                btnRegister.Enabled = false;

                // Authenticate user
                var result = await _userService.AuthenticateAsync(username, password);

                if (result.Success && result.User != null)
                {
                    // Authentication successful
                    lblStatus.Text = string.Empty;

                    // Open appropriate form based on user type
                    Form mainForm = null;

                    if (result.User.IsAdmin())
                    {
                        mainForm = ControlFactory.CreateAdminForm(result.User);
                    }
                    else if (result.User.IsDriver())
                    {
                        mainForm = ControlFactory.CreateDriverForm(result.User);
                    }
                    else if (result.User.IsPassenger())
                    {
                        mainForm = ControlFactory.CreatePassengerForm(result.User);
                    }

                    if (mainForm != null)
                    {
                        this.Hide();
                        mainForm.FormClosed += (s, args) => this.Close();
                        mainForm.Show();
                    }
                    else
                    {
                        lblStatus.ForeColor = Color.Red;
                        lblStatus.Text = "Error: Unknown user type.";
                        btnLogin.Enabled = true;
                        btnRegister.Enabled = true;
                    }
                }
                else
                {
                    // Authentication failed
                    lblStatus.ForeColor = Color.Red;
                    lblStatus.Text = "Invalid username or password.";
                    btnLogin.Enabled = true;
                    btnRegister.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = $"Error: {ex.Message}";
                btnLogin.Enabled = true;
                btnRegister.Enabled = true;
            }
        }

        private void ShowRegistrationForm()
        {
            // Open registration form
            var registrationForm = ControlFactory.CreateRegistrationForm(_userService);
            registrationForm.ShowDialog(this);

            // If registration is successful, populate the username
            if (registrationForm.DialogResult == DialogResult.OK && !string.IsNullOrEmpty(registrationForm.Username))
            {
                txtUsername.Text = registrationForm.Username;
                txtPassword.Focus();
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Adjust panel size
            mainPanel.Size = this.ClientSize;
            mainPanel.Location = new Point(0, 0);

            // Default to demo credentials for testing
#if DEBUG
            txtUsername.Text = "driver1";
            txtPassword.Text = "password";
#endif
        }
    }
}