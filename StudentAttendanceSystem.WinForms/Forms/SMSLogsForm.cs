using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class SMSLogsForm : Form
    {
        private readonly SMSRepository _smsRepository;
        private DataGridView dgvSMSLogs;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private ComboBox cmbStatus;
        private Button btnSearch;
        private Button btnRefresh;
        private Button btnExport;
        private Button btnClose;
        private Label lblTotalRecords;
        private GroupBox gbFilters;

        public SMSLogsForm()
        {
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _smsRepository = new SMSRepository(dbConnection);
            InitializeComponent();
            LoadSMSLogs();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "SMS Logs Viewer";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Filters Group
            gbFilters = new GroupBox
            {
                Text = "Filter Options",
                Location = new Point(20, 20),
                Size = new Size(940, 80),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            var lblFromDate = new Label
            {
                Text = "From Date:",
                Location = new Point(15, 25),
                Size = new Size(70, 20),
                Font = new Font("Arial", 9)
            };

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(90, 23),
                Size = new Size(120, 25),
                Font = new Font("Arial", 9),
                Value = DateTime.Today.AddDays(-7) // Default to last 7 days
            };

            var lblToDate = new Label
            {
                Text = "To Date:",
                Location = new Point(220, 25),
                Size = new Size(60, 20),
                Font = new Font("Arial", 9)
            };

            dtpToDate = new DateTimePicker
            {
                Location = new Point(285, 23),
                Size = new Size(120, 25),
                Font = new Font("Arial", 9),
                Value = DateTime.Today.AddDays(1)
            };

            var lblStatus = new Label
            {
                Text = "Status:",
                Location = new Point(420, 25),
                Size = new Size(50, 20),
                Font = new Font("Arial", 9)
            };

            cmbStatus = new ComboBox
            {
                Location = new Point(475, 23),
                Size = new Size(100, 25),
                Font = new Font("Arial", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new[] { "All", "Pending", "Sent", "Failed", "Delivered", "Queued" });
            cmbStatus.SelectedIndex = 0; // Select "All"

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(590, 22),
                Size = new Size(80, 27),
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnSearch.Click += BtnSearch_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(680, 22),
                Size = new Size(80, 27),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnRefresh.Click += BtnRefresh_Click;

            btnExport = new Button
            {
                Text = "Export CSV",
                Location = new Point(770, 22),
                Size = new Size(80, 27),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnExport.Click += BtnExport_Click;

            gbFilters.Controls.AddRange(new Control[] {
                lblFromDate, dtpFromDate, lblToDate, dtpToDate, lblStatus, cmbStatus,
                btnSearch, btnRefresh, btnExport
            });

            // Data Grid
            dgvSMSLogs = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(940, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };

            // Status and action buttons
            lblTotalRecords = new Label
            {
                Text = "Total Records: 0",
                Location = new Point(20, 520),
                Size = new Size(200, 20),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(880, 520),
                Size = new Size(80, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnClose.Click += BtnClose_Click;

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                gbFilters, dgvSMSLogs, lblTotalRecords, btnClose
            });

            this.ResumeLayout(false);
        }

        private async Task LoadSMSLogs()
        {
            try
            {
                var fromDate = dtpFromDate.Value.Date;
                var toDate = dtpToDate.Value.Date.AddDays(1);

                var logs = await _smsRepository.GetSMSLogsAsync(null, fromDate, toDate, 1000);

                // Filter by status if not "All"
                if (cmbStatus.SelectedIndex > 0)
                {
                    var selectedStatus = (SMSStatus)(cmbStatus.SelectedIndex);
                    logs = logs.Where(l =>
                    {
                        return l.Status.Equals(selectedStatus);
                    }).ToList();
                }

                var displayData = logs.Select(log => new
                {
                    LogId = log.LogId,
                    StudentId = log.StudentId,
                    PhoneNumber = log.PhoneNumber,
                    Message = log.Message.Length > 50 ? log.Message.Substring(0, 50) + "..." : log.Message,
                    FullMessage = log.Message,
                    Status = GetStatusText((SMSStatus)log.Status),
                    StatusColor = GetStatusColor((SMSStatus)log.Status),
                    SentDate = log.SentDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ErrorMessage = log.ErrorMessage ?? ""
                }).ToList();

                dgvSMSLogs.DataSource = displayData;

                // Hide some columns
                if (dgvSMSLogs.Columns["LogId"] != null)
                    dgvSMSLogs.Columns["LogId"].Visible = false;
                if (dgvSMSLogs.Columns["FullMessage"] != null)
                    dgvSMSLogs.Columns["FullMessage"].Visible = false;
                if (dgvSMSLogs.Columns["StatusColor"] != null)
                    dgvSMSLogs.Columns["StatusColor"].Visible = false;

                // Format columns
                if (dgvSMSLogs.Columns["StudentId"] != null)
                {
                    dgvSMSLogs.Columns["StudentId"].HeaderText = "Student ID";
                    dgvSMSLogs.Columns["StudentId"].Width = 80;
                }
                if (dgvSMSLogs.Columns["PhoneNumber"] != null)
                {
                    dgvSMSLogs.Columns["PhoneNumber"].HeaderText = "Phone Number";
                    dgvSMSLogs.Columns["PhoneNumber"].Width = 120;
                }
                if (dgvSMSLogs.Columns["Message"] != null)
                {
                    dgvSMSLogs.Columns["Message"].HeaderText = "Message Preview";
                    dgvSMSLogs.Columns["Message"].Width = 300;
                }
                if (dgvSMSLogs.Columns["Status"] != null)
                {
                    dgvSMSLogs.Columns["Status"].HeaderText = "Status";
                    dgvSMSLogs.Columns["Status"].Width = 80;
                }
                if (dgvSMSLogs.Columns["SentDate"] != null)
                {
                    dgvSMSLogs.Columns["SentDate"].HeaderText = "Sent Date";
                    dgvSMSLogs.Columns["SentDate"].Width = 140;
                }
                if (dgvSMSLogs.Columns["ErrorMessage"] != null)
                {
                    dgvSMSLogs.Columns["ErrorMessage"].HeaderText = "Error";
                    dgvSMSLogs.Columns["ErrorMessage"].Width = 200;
                }

                // Color code rows based on status
                dgvSMSLogs.CellFormatting += DgvSMSLogs_CellFormatting;
                dgvSMSLogs.CellDoubleClick += DgvSMSLogs_CellDoubleClick;

                lblTotalRecords.Text = $"Total Records: {displayData.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading SMS logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvSMSLogs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvSMSLogs.Columns[e.ColumnIndex].Name == "Status" && e.RowIndex >= 0)
            {
                var status = e.Value?.ToString();
                switch (status)
                {
                    case "Sent":
                    case "Delivered":
                        dgvSMSLogs.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                        break;
                    case "Failed":
                        dgvSMSLogs.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                        break;
                    case "Pending":
                    case "Queued":
                        dgvSMSLogs.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                        break;
                }
            }
        }

        private void DgvSMSLogs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var fullMessage = dgvSMSLogs.Rows[e.RowIndex].Cells["FullMessage"].Value?.ToString();
                var phoneNumber = dgvSMSLogs.Rows[e.RowIndex].Cells["PhoneNumber"].Value?.ToString();
                var sentDate = dgvSMSLogs.Rows[e.RowIndex].Cells["SentDate"].Value?.ToString();
                var status = dgvSMSLogs.Rows[e.RowIndex].Cells["Status"].Value?.ToString();
                var errorMessage = dgvSMSLogs.Rows[e.RowIndex].Cells["ErrorMessage"].Value?.ToString();

                var details = $"Phone Number: {phoneNumber}\n";
                details += $"Sent Date: {sentDate}\n";
                details += $"Status: {status}\n\n";
                details += $"Full Message:\n{fullMessage}";
                
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    details += $"\n\nError Message:\n{errorMessage}";
                }

                MessageBox.Show(details, "SMS Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GetStatusText(SMSStatus status)
        {
            return status switch
            {
                SMSStatus.Pending => "Pending",
                SMSStatus.Sent => "Sent",
                SMSStatus.Failed => "Failed",
                SMSStatus.Delivered => "Delivered",
                SMSStatus.Queued => "Queued",
                _ => "Unknown"
            };
        }

        private Color GetStatusColor(SMSStatus status)
        {
            return status switch
            {
                SMSStatus.Sent or SMSStatus.Delivered => Color.Green,
                SMSStatus.Failed => Color.Red,
                SMSStatus.Pending or SMSStatus.Queued => Color.Orange,
                _ => Color.Gray
            };
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            await LoadSMSLogs();
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await LoadSMSLogs();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"SMS_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToCSV(saveFileDialog.FileName);
                    MessageBox.Show($"SMS logs exported successfully to:\n{saveFileDialog.FileName}", 
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting SMS logs: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(string fileName)
        {
            using var writer = new StreamWriter(fileName);
            
            // Write headers
            var headers = new[] { "Student ID", "Phone Number", "Full Message", "Status", "Sent Date", "Error Message" };
            writer.WriteLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

            // Write data
            foreach (DataGridViewRow row in dgvSMSLogs.Rows)
            {
                if (!row.IsNewRow)
                {
                    var values = new[]
                    {
                        row.Cells["StudentId"].Value?.ToString() ?? "",
                        row.Cells["PhoneNumber"].Value?.ToString() ?? "",
                        row.Cells["FullMessage"].Value?.ToString()?.Replace("\"", "\"\"") ?? "",
                        row.Cells["Status"].Value?.ToString() ?? "",
                        row.Cells["SentDate"].Value?.ToString() ?? "",
                        row.Cells["ErrorMessage"].Value?.ToString()?.Replace("\"", "\"\"") ?? ""
                    };
                    
                    writer.WriteLine(string.Join(",", values.Select(v => $"\"{v}\"")));
                }
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}