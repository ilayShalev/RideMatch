using System;
using System.Drawing;
using System.Windows.Forms;

namespace RideMatch.UI.Forms
{
    partial class AdminForm
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
            this.tabControl = new TabControl();
            this.statusStrip = new StatusStrip();
            this.statusLabel = new ToolStripStatusLabel();
            this.toolStrip = new ToolStrip();

            this.SuspendLayout();

            // Set form properties
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);

            // Status strip
            this.statusStrip.Items.AddRange(new ToolStripItem[] { this.statusLabel });
            this.statusStrip.Location = new Point(0, this.ClientSize.Height - 22);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new Size(this.ClientSize.Width, 22);
            this.Controls.Add(this.statusStrip);

            // Tool strip
            this.toolStrip.Items.AddRange(new ToolStripItem[] {
            new ToolStripButton("Refresh", null, (s, e) => _ = LoadAllDataAsync()) { DisplayStyle = ToolStripItemDisplayStyle.Text },
            new ToolStripSeparator(),
            new ToolStripButton("Add User", null, (s, e) => ShowAddUserDialog()) { DisplayStyle = ToolStripItemDisplayStyle.Text },
            new ToolStripSeparator(),
            new ToolStripButton("Logout", null, (s, e) => Logout()) { DisplayStyle = ToolStripItemDisplayStyle.Text, Alignment = ToolStripItemAlignment.Right }
             });
            this.toolStrip.Location = new Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new Size(this.ClientSize.Width, 25);
            this.Controls.Add(this.toolStrip);

            // Tab control
            this.tabControl.Dock = DockStyle.Fill;
            this.tabControl.Location = new Point(0, 25); // Below toolbar
            this.tabControl.Name = "tabControl";
            this.tabControl.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 47);
            this.tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            this.Controls.Add(this.tabControl);

            // Wire up the load event
            this.Load += AdminForm_Load;

            this.ResumeLayout(false);
            this.PerformLayout();
        }



        private void AdminForm_Load(object sender, EventArgs e)
        {
            // Load data when form loads
            _ = LoadAllDataAsync();
        }
        #endregion
    }
}