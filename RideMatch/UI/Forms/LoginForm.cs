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
    public class LoginForm : Form
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

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "RideMatch - Login";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Main panel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            this.Controls.Add(mainPanel);

            // Logo/App Title
            logoBox = new PictureBox
            {
                Size = new Size(200, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point((mainPanel.Width - 200) / 2, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            // Since we don't have an actual logo, we'll use a label instead
            Label lblLogo = new Label
            {
                Text = "RideMatch",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 50),
                Location = new Point(0, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            mainPanel.Controls.Add(lblLogo);

            // App description
            Label lblDescription = new Label
            {
                Text = "Intelligent Ridesharing Solution",
                Font = new Font("Arial", 12, FontStyle.Italic),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 30),
                Location = new Point(0, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            mainPanel.Controls.Add(lblDescription);

            // Username label
            Label lblUsername = new Label
            {
                Text = "Username:",
                Size = new Size(360, 20),
                Location = new Point(0, 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            mainPanel.Controls.Add(lblUsername);

            // Username textbox
            txtUsername = new TextBox
            {
                Size = new Size(360, 30),
                Location = new Point(0, 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Arial", 12)
            };
            mainPanel.Controls.Add(txtUsername);

            // Password label
            Label lblPassword = new Label
            {
                Text = "Password:",
                Size = new Size(360, 20),
                Location = new Point(0, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            mainPanel.Controls.Add(lblPassword);

            // Password textbox
            txtPassword = new TextBox
            {
                Size = new Size(360, 30),
                Location = new Point(0, 220),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Arial", 12),
                PasswordChar = '*'
            };
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _ = LoginAsync();
                }
            };
            mainPanel.Controls.Add(txtPassword);

            // Login button
            btnLogin = new Button
            {
                Text = "Login",
                Size = new Size(360, 40),
                Location = new Point(0, 270),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += async (s, e) => await LoginAsync();
            mainPanel.Controls.Add(btnLogin);

            // Register button
            btnRegister = new Button
            {
                Text = "New User? Register Here",
                Size = new Size(360, 30),
                Location = new Point(0, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent,
                ForeColor = Color.DodgerBlue,
                Font = new Font("Arial", 10),
                FlatStyle = FlatStyle.Flat
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += (s, e) => ShowRegistrationForm();
            mainPanel.Controls.Add(btnRegister);

            // Status label
            lblStatus = new Label
            {
                Size = new Size(360, 20),
                Location = new Point(0, 360),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblStatus);
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