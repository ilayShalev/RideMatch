using System.Drawing;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
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

            // Wire up the load event
            this.Load += LoginForm_Load;
        }
        #endregion
    }
}