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
            LoadData();
        }

        private void InitializeUI()
        {
            // Configure DataGridViews
            ConfigureDataGridView(dataGridViewDrivers);
            ConfigureDataGridView(dataGridViewPassengers);
            ConfigureDataGridView(dataGridViewResults);

            // Wire up event handlers
            btnAddDriver.Click += BtnAddDriver_Click;
            btnAddPassenger.Click += BtnAddPassenger_Click;
            btnRunAlgorithm.Click += BtnRunAlgorithm_Click;
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
        }

        private void ConfigureDataGridView(DataGridView dgv)
        {
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadData()
        {
            try
            {
                var drivers = _databaseService.GetAllDrivers();
                var passengers = _databaseService.GetAllPassengers();

                dataGridViewDrivers.DataSource = drivers;
                dataGridViewPassengers.DataSource = passengers;

                toolStripStatusLabel1.Text = $"נטען: {drivers.Count} נהגים, {passengers.Count} נוסעים";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת נתונים: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddDriver_Click(object sender, EventArgs e)
        {
            // TODO: Implement add driver form
            MessageBox.Show("פונקציונליות הוספת נהג תתווסף בקרוב", "מידע", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAddPassenger_Click(object sender, EventArgs e)
        {
            // TODO: Implement add passenger form
            MessageBox.Show("פונקציונליות הוספת נוסע תתווסף בקרוב", "מידע", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRunAlgorithm_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _geneticAlgorithm.RunAlgorithm();
                LoadData(); // Reload data to show new assignments
                MessageBox.Show("האלגוריתם הושלם בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בהפעלת האלגוריתם: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "RideMatch - מערכת הסעות חכמה\n" +
                "גרסה 1.0\n" +
                "פותח על ידי אלעאי שלו\n" +
                "© 2024",
                "אודות",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Form load event is handled in constructor
        }
    }
} 