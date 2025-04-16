using System.Drawing;
using System.Windows.Forms;

namespace RideMatch.UI.Controls
{
    partial class AddressSearchControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AddressSearchControl
            // 
            this.Name = "AddressSearchControl";
            this.Load += new System.EventHandler(this.AddressSearchControl_Load_1);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
