using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.UI.Helpers
{
    public static class DialogHelper
    {
        // Shows an error message
        public static void ShowError(string message, string title = "Error");

        // Shows a warning message
        public static void ShowWarning(string message, string title = "Warning");

        // Shows an information message
        public static void ShowInfo(string message, string title = "Information");

        // Shows a confirmation dialog
        public static bool Confirm(string message, string title = "Confirm");

        // Shows an input dialog
        public static string GetInput(string prompt, string title = "Input", string defaultValue = "");
    }
}
