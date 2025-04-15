using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Controls
{
    public class AddressSearchControl : UserControl
    {
        // Initializes components
        private void InitializeComponent();

        // Searches for an address
        public async Task SearchAddressAsync();

        // Shows a status message
        private void ShowStatus(string message, bool success);

        // Raises the AddressFound event
        protected virtual void OnAddressFound(AddressFoundEventArgs e);
    }
}
