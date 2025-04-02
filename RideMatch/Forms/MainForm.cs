using System;
using System.Windows.Forms;
using RideMatch.Models;
using RideMatch.Services;

namespace RideMatch.Forms
{
    public partial class MainForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly GeneticAlgorithm _geneticAlgorithm;

        public MainForm()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _geneticAlgorithm = new GeneticAlgorithm(_databaseService);
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Add UI initialization code here
            this.Text = "RideMatch - מערכת הסעות חכמה";
            this.Size = new System.Drawing.Size(1024, 768);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Add form load event handling code here
        }
    }
} 