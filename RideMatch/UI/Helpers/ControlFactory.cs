using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RideMatch.UI.Helpers
{
    public static class ControlFactory
    {
        // Creates a label control
        public static Label CreateLabel(string text, Point location, Size size, Font font = null, ContentAlignment alignment = ContentAlignment.MiddleLeft);

        // Creates a button control
        public static Button CreateButton(string text, Point location, Size size, EventHandler onClick = null);

        // Creates a text box control
        public static TextBox CreateTextBox(Point location, Size size, string text = "", bool multiline = false, bool readOnly = false);

        // Creates a rich text box
        public static RichTextBox CreateRichTextBox(Point location, Size size, bool readOnly = false);

        // Creates a numeric up/down control
        public static NumericUpDown CreateNumericUpDown(Point location, Size size, decimal min, decimal max, decimal value, decimal increment = 1);

        // Creates a combo box
        public static ComboBox CreateComboBox(Point location, Size size, string[] items, int selectedIndex = 0);

        // Creates a check box
        public static CheckBox CreateCheckBox(string text, Point location, Size size, bool isChecked = false);

        // Creates a radio button
        public static RadioButton CreateRadioButton(string text, Point location, Size size, bool isChecked = false);

        // Creates a panel
        public static Panel CreatePanel(Point location, Size size, BorderStyle borderStyle = BorderStyle.None);

        // Appends a log message to a text box
        public static void AppendLog(this TextBox textBox, string message);
    }
}
