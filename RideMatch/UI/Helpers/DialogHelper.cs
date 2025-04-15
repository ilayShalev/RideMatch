using System;
using System.Windows.Forms;

namespace RideMatch.UI.Helpers
{
    public static class DialogHelper
    {
        // Shows an error message
        public static void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Shows a warning message
        public static void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Shows an information message
        public static void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Shows a confirmation dialog
        public static bool Confirm(string message, string title = "Confirm")
        {
            DialogResult result = MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        // Shows an input dialog
        public static string GetInput(string prompt, string title = "Input", string defaultValue = "")
        {
            // Create input form
            Form inputForm = new Form
            {
                Text = title,
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // Create label
            Label label = new Label
            {
                Text = prompt,
                Left = 10,
                Top = 20,
                Width = 280
            };
            inputForm.Controls.Add(label);

            // Create textbox
            TextBox textBox = new TextBox
            {
                Left = 10,
                Top = 50,
                Width = 280,
                Text = defaultValue
            };
            inputForm.Controls.Add(textBox);

            // Create OK button
            Button okButton = new Button
            {
                Text = "OK",
                Left = 120,
                Top = 80,
                Width = 80,
                DialogResult = DialogResult.OK
            };
            inputForm.Controls.Add(okButton);

            // Set OK button as accept button
            inputForm.AcceptButton = okButton;

            // Show form
            DialogResult result = inputForm.ShowDialog();

            // Return input if OK was clicked
            if (result == DialogResult.OK)
            {
                return textBox.Text;
            }

            return null;
        }
    }
}