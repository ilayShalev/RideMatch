using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    public class LoginForm : Form
    {
        // Initializes UI components
        private void InitializeComponent();

        // Sets up UI controls
        private void SetupUI();

        // Handles login attempt
        private async Task LoginAsync();

        // Shows the registration form
        private void ShowRegistrationForm();
    }
}
