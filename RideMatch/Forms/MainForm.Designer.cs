namespace RideMatch.Forms
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabDrivers = new System.Windows.Forms.TabPage();
            this.tabPassengers = new System.Windows.Forms.TabPage();
            this.tabResults = new System.Windows.Forms.TabPage();
            this.dataGridViewDrivers = new System.Windows.Forms.DataGridView();
            this.dataGridViewPassengers = new System.Windows.Forms.DataGridView();
            this.dataGridViewResults = new System.Windows.Forms.DataGridView();
            this.btnAddDriver = new System.Windows.Forms.Button();
            this.btnAddPassenger = new System.Windows.Forms.Button();
            this.btnRunAlgorithm = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1.SuspendLayout();
            this.tabDrivers.SuspendLayout();
            this.tabPassengers.SuspendLayout();
            this.tabResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDrivers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPassengers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResults)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabDrivers);
            this.tabControl1.Controls.Add(this.tabPassengers);
            this.tabControl1.Controls.Add(this.tabResults);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tabControl1.RightToLeftLayout = true;
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1024, 744);
            this.tabControl1.TabIndex = 0;
            // 
            // tabDrivers
            // 
            this.tabDrivers.Controls.Add(this.dataGridViewDrivers);
            this.tabDrivers.Controls.Add(this.btnAddDriver);
            this.tabDrivers.Location = new System.Drawing.Point(4, 22);
            this.tabDrivers.Name = "tabDrivers";
            this.tabDrivers.Padding = new System.Windows.Forms.Padding(3);
            this.tabDrivers.Size = new System.Drawing.Size(1016, 718);
            this.tabDrivers.TabIndex = 0;
            this.tabDrivers.Text = "נהגים";
            this.tabDrivers.UseVisualStyleBackColor = true;
            // 
            // tabPassengers
            // 
            this.tabPassengers.Controls.Add(this.dataGridViewPassengers);
            this.tabPassengers.Controls.Add(this.btnAddPassenger);
            this.tabPassengers.Location = new System.Drawing.Point(4, 22);
            this.tabPassengers.Name = "tabPassengers";
            this.tabPassengers.Padding = new System.Windows.Forms.Padding(3);
            this.tabPassengers.Size = new System.Drawing.Size(1016, 718);
            this.tabPassengers.TabIndex = 1;
            this.tabPassengers.Text = "נוסעים";
            this.tabPassengers.UseVisualStyleBackColor = true;
            // 
            // tabResults
            // 
            this.tabResults.Controls.Add(this.dataGridViewResults);
            this.tabResults.Controls.Add(this.btnRunAlgorithm);
            this.tabResults.Location = new System.Drawing.Point(4, 22);
            this.tabResults.Name = "tabResults";
            this.tabResults.Padding = new System.Windows.Forms.Padding(3);
            this.tabResults.Size = new System.Drawing.Size(1016, 718);
            this.tabResults.TabIndex = 2;
            this.tabResults.Text = "תוצאות";
            this.tabResults.UseVisualStyleBackColor = true;
            // 
            // dataGridViewDrivers
            // 
            this.dataGridViewDrivers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewDrivers.Location = new System.Drawing.Point(6, 41);
            this.dataGridViewDrivers.Name = "dataGridViewDrivers";
            this.dataGridViewDrivers.Size = new System.Drawing.Size(1004, 671);
            this.dataGridViewDrivers.TabIndex = 1;
            // 
            // dataGridViewPassengers
            // 
            this.dataGridViewPassengers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewPassengers.Location = new System.Drawing.Point(6, 41);
            this.dataGridViewPassengers.Name = "dataGridViewPassengers";
            this.dataGridViewPassengers.Size = new System.Drawing.Size(1004, 671);
            this.dataGridViewPassengers.TabIndex = 1;
            // 
            // dataGridViewResults
            // 
            this.dataGridViewResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewResults.Location = new System.Drawing.Point(6, 41);
            this.dataGridViewResults.Name = "dataGridViewResults";
            this.dataGridViewResults.Size = new System.Drawing.Size(1004, 671);
            this.dataGridViewResults.TabIndex = 1;
            // 
            // btnAddDriver
            // 
            this.btnAddDriver.Location = new System.Drawing.Point(6, 6);
            this.btnAddDriver.Name = "btnAddDriver";
            this.btnAddDriver.Size = new System.Drawing.Size(75, 23);
            this.btnAddDriver.TabIndex = 0;
            this.btnAddDriver.Text = "הוסף נהג";
            this.btnAddDriver.UseVisualStyleBackColor = true;
            // 
            // btnAddPassenger
            // 
            this.btnAddPassenger.Location = new System.Drawing.Point(6, 6);
            this.btnAddPassenger.Name = "btnAddPassenger";
            this.btnAddPassenger.Size = new System.Drawing.Size(75, 23);
            this.btnAddPassenger.TabIndex = 0;
            this.btnAddPassenger.Text = "הוסף נוסע";
            this.btnAddPassenger.UseVisualStyleBackColor = true;
            // 
            // btnRunAlgorithm
            // 
            this.btnRunAlgorithm.Location = new System.Drawing.Point(6, 6);
            this.btnRunAlgorithm.Name = "btnRunAlgorithm";
            this.btnRunAlgorithm.Size = new System.Drawing.Size(75, 23);
            this.btnRunAlgorithm.TabIndex = 0;
            this.btnRunAlgorithm.Text = "הפעל אלגוריתם";
            this.btnRunAlgorithm.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.fileToolStripMenuItem,
                this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1024, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.fileToolStripMenuItem.Text = "קובץ";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "יציאה";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.helpToolStripMenuItem.Text = "עזרה";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "אודות";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 768);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1024, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 790);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Text = "RideMatch - מערכת הסעות חכמה";
            this.tabControl1.ResumeLayout(false);
            this.tabDrivers.ResumeLayout(false);
            this.tabPassengers.ResumeLayout(false);
            this.tabResults.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDrivers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPassengers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResults)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabDrivers;
        private System.Windows.Forms.TabPage tabPassengers;
        private System.Windows.Forms.TabPage tabResults;
        private System.Windows.Forms.DataGridView dataGridViewDrivers;
        private System.Windows.Forms.DataGridView dataGridViewPassengers;
        private System.Windows.Forms.DataGridView dataGridViewResults;
        private System.Windows.Forms.Button btnAddDriver;
        private System.Windows.Forms.Button btnAddPassenger;
        private System.Windows.Forms.Button btnRunAlgorithm;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
} 