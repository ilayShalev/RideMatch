using System;
using System.Windows.Forms;
using RideMatch.Models;
using RideMatch.Services;
using System.Linq;

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
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToResizeRows = false;
            dgv.BackgroundColor = System.Drawing.SystemColors.Window;
            dgv.BorderStyle = BorderStyle.None;
            dgv.AutoGenerateColumns = false;

            // Clear existing columns
            dgv.Columns.Clear();

            // Add columns based on grid type
            if (dgv == dataGridViewDrivers)
            {
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Id",
                    Name = "Id",
                    HeaderText = "מזהה",
                    Width = 50
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Name",
                    Name = "Name",
                    HeaderText = "שם",
                    Width = 150
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "PhoneNumber",
                    Name = "PhoneNumber",
                    HeaderText = "טלפון",
                    Width = 100
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Address",
                    Name = "Address",
                    HeaderText = "כתובת",
                    Width = 200
                });

                dgv.Columns.Add(new DataGridViewCheckBoxColumn
                {
                    DataPropertyName = "IsAvailable",
                    Name = "IsAvailable",
                    HeaderText = "זמין",
                    Width = 50
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "LastUpdate",
                    Name = "LastUpdate",
                    HeaderText = "עדכון אחרון",
                    Width = 120
                });
            }
            else if (dgv == dataGridViewPassengers)
            {
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Id",
                    Name = "Id",
                    HeaderText = "מזהה",
                    Width = 50
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Name",
                    Name = "Name",
                    HeaderText = "שם",
                    Width = 150
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "PhoneNumber",
                    Name = "PhoneNumber",
                    HeaderText = "טלפון",
                    Width = 100
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Address",
                    Name = "Address",
                    HeaderText = "כתובת",
                    Width = 200
                });

                dgv.Columns.Add(new DataGridViewCheckBoxColumn
                {
                    DataPropertyName = "IsAvailable",
                    Name = "IsAvailable",
                    HeaderText = "זמין",
                    Width = 50
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "AssignedDriverId",
                    Name = "AssignedDriverId",
                    HeaderText = "נהג",
                    Width = 50
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "PickupTime",
                    Name = "PickupTime",
                    HeaderText = "זמן איסוף",
                    Width = 120
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "LastUpdate",
                    Name = "LastUpdate",
                    HeaderText = "עדכון אחרון",
                    Width = 120
                });
            }
            else if (dgv == dataGridViewResults)
            {
                // Configure results grid columns
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "DriverName",
                    HeaderText = "נהג",
                    Width = 150
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "PassengerName",
                    HeaderText = "נוסע",
                    Width = 150
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "PickupTime",
                    HeaderText = "זמן איסוף",
                    Width = 120
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "PassengerAddress",
                    HeaderText = "כתובת איסוף",
                    Width = 200
                });
            }

            // Set column styles
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void LoadData()
        {
            try
            {
                var drivers = _databaseService.GetAllDrivers();
                var passengers = _databaseService.GetAllPassengers();

                dataGridViewDrivers.DataSource = drivers;
                dataGridViewPassengers.DataSource = passengers;

                // Clear existing data source
                dataGridViewResults.DataSource = null;

                // Create results data with matching column names
                var results = from p in passengers
                             where p.AssignedDriverId.HasValue
                             join d in drivers on p.AssignedDriverId equals d.Id
                             select new
                             {
                                 DriverName = d.Name,
                                 PassengerName = p.Name,
                                 PickupTime = p.PickupTime?.ToString("HH:mm") ?? "",
                                 PassengerAddress = p.Address
                             };

                var resultsList = results.ToList();
                dataGridViewResults.DataSource = resultsList;

                toolStripStatusLabel1.Text = $"נטען: {drivers.Count} נהגים, {passengers.Count} נוסעים";

                if (resultsList.Any())
                {
                    tabControl1.SelectedTab = tabResults;
                }
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