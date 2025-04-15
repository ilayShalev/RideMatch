using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Models
{
    public class SchedulingLogEntry
    {
        // Properties for run time, status, routes generated, passengers assigned, etc.

        // Formatted date/time for display
        public string FormattedDateTime();

        // Status with color information for UI
        public (string Status, Color Color) GetStatusWithColor();
    }
}
