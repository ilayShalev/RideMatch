using RideMatch.Core.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    partial class RegistrationForm
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
            this.Text = "Register New User";
            this.Size = new Size(400, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

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

            // Set accept/cancel buttons
            this.AcceptButton = btnRegister;
            this.CancelButton = btnCancel;

            // Wire up load event
            this.Load += RegistrationForm_Load;
        }



        private void RegistrationForm_Load(object sender, EventArgs e)
        {
            // Empty implementation - keeps the designer happy
        }
        #endregion
    }
}