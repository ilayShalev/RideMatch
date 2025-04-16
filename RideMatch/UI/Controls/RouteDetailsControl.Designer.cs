using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RideMatch.UI.Controls
{
    partial class RouteDetailsControl
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
            // RouteDetailsControl
            // 
            this.Name = "RouteDetailsControl";
            this.Load += new System.EventHandler(this.RouteDetailsControl_Load);
            this.ResumeLayout(false);

        }


        private void RouteDetailsControl_Load(object sender, EventArgs e)
        {
            // Empty implementation - keeps the designer happy
        }
        #endregion
    }
}
