using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class MainForm : Form
    {
        private readonly User _currentUser;
        private readonly StudentRepository _studentRepository;
        private MenuStrip menuStrip;
        private StatusStrip statusStrip;
        private DataGridView dgvStudents;
        private ToolStripStatusLabel lblUser;
        private ToolStripStatusLabel lblDateTime;
        private System.Windows.Forms.Timer timeTimer;

        public MainForm(User currentUser)
        {
            _currentUser = currentUser;
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _studentRepository = new StudentRepository(dbConnection);
            InitializeComponent();
            LoadStudentData();
            SetupTimer();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Student Attendance System - Main";
            this.WindowState = FormWindowState.Maximized;
            this.IsMdiContainer = true;

            // Create menu strip
            CreateMenuStrip();

            // Create status strip
            CreateStatusStrip();

            // Create main data grid
            CreateDataGrid();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateMenuStrip()
        {
            menuStrip = new MenuStrip();

            if (_currentUser.Role == UserRole.Administrator)
            {
                // Administrator menu items
                var userMenu = new ToolStripMenuItem("User");
                userMenu.DropDownItems.Add("Manage Users", null, ManageUsers_Click);

                var studentMenu = new ToolStripMenuItem("Student");
                studentMenu.DropDownItems.Add("Manage Students", null, ManageStudents_Click);

                var guardianMenu = new ToolStripMenuItem("Guardian");
                guardianMenu.DropDownItems.Add("Manage Guardians", null, ManageGuardians_Click);

                var rfidMenu = new ToolStripMenuItem("RFID");
                rfidMenu.DropDownItems.Add("Reader Configuration", null, RFIDReader_Click);
                rfidMenu.DropDownItems.Add("Generate RFID", null, GenerateRFID_Click);

                var smsMenu = new ToolStripMenuItem("SMS Provider");
                smsMenu.DropDownItems.Add("Configure SMS", null, ConfigureSMS_Click);

                menuStrip.Items.AddRange(new ToolStripItem[] 
                { 
                    userMenu, studentMenu, guardianMenu, rfidMenu, smsMenu 
                });
            }
            else
            {
                // Regular user menu items
                var studentMenu = new ToolStripMenuItem("Student");
                studentMenu.DropDownItems.Add("View/Update Students", null, ViewUpdateStudents_Click);

                var guardianMenu = new ToolStripMenuItem("Guardian");
                guardianMenu.DropDownItems.Add("View/Update Guardians", null, ViewUpdateGuardians_Click);

                menuStrip.Items.AddRange(new ToolStripItem[] 
                { 
                    studentMenu, guardianMenu 
                });
            }

            // Common menu items
            var systemMenu = new ToolStripMenuItem("System");
            systemMenu.DropDownItems.Add("Log Off", null, LogOff_Click);
            systemMenu.DropDownItems.Add("Exit", null, Exit_Click);
            menuStrip.Items.Add(systemMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void CreateStatusStrip()
        {
            statusStrip = new StatusStrip();

            lblUser = new ToolStripStatusLabel($"Logged in as: {_currentUser.FirstName} {_currentUser.LastName} ({_currentUser.Role})");
            lblUser.Spring = true;
            lblUser.TextAlign = ContentAlignment.MiddleLeft;

            lblDateTime = new ToolStripStatusLabel(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            lblDateTime.TextAlign = ContentAlignment.MiddleRight;

            statusStrip.Items.AddRange(new ToolStripItem[] { lblUser, lblDateTime });
            this.Controls.Add(statusStrip);
        }

        private void CreateDataGrid()
        {
            dgvStudents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White
            };

            dgvStudents.DoubleClick += DgvStudents_DoubleClick;
            this.Controls.Add(dgvStudents);
        }

        private async void LoadStudentData()
        {
            try
            {
                var students = await _studentRepository.GetAllStudentsAsync();
                
                var displayData = students.Select(s => new
                {
                    ID = s.StudentId,
                    StudentNumber = s.StudentNumber,
                    FirstName = s.FirstName,
                    MiddleName = s.MiddleName,
                    LastName = s.LastName,
                    CellPhone = s.CellPhone,
                    Email = s.Email,
                    GuardianFirstName = s.Guardian?.FirstName ?? "",
                    GuardianLastName = s.Guardian?.LastName ?? "",
                    GuardianCellPhone = s.Guardian?.CellPhone ?? "",
                    GuardianEmail = s.Guardian?.Email ?? "",
                    TimeInOut = "Not Available" // This would come from attendance records
                }).ToList();

                dgvStudents.DataSource = displayData;

                // Hide the ID column but keep it for reference
                if (dgvStudents.Columns["ID"] != null)
                    dgvStudents.Columns["ID"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupTimer()
        {
            timeTimer = new System.Windows.Forms.Timer();
            timeTimer.Interval = 1000; // Update every second
            timeTimer.Tick += TimeTimer_Tick;
            timeTimer.Start();
        }

        private void TimeTimer_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private async void DgvStudents_DoubleClick(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow?.Index >= 0)
            {
                var studentId = (int)dgvStudents.CurrentRow.Cells["ID"].Value;
                var student = await _studentRepository.GetStudentByIdAsync(studentId);
                
                if (student != null)
                {
                    var detailForm = new StudentDetailForm(student);
                    detailForm.ShowDialog();
                }
            }
        }

        // Menu event handlers
        private void ManageUsers_Click(object sender, EventArgs e)
        {
            var userManagementForm = new UserManagementForm(_currentUser);
            userManagementForm.ShowDialog();
        }

        private void ManageStudents_Click(object sender, EventArgs e)
        {
            var studentManagementForm = new StudentManagementForm();
            studentManagementForm.FormClosed += (s, args) => LoadStudentData();
            studentManagementForm.ShowDialog();
        }

        private void ManageGuardians_Click(object sender, EventArgs e)
        {
            var guardianManagementForm = new GuardianManagementForm();
            guardianManagementForm.FormClosed += (s, args) => LoadStudentData();
            guardianManagementForm.ShowDialog();
        }

        private void RFIDReader_Click(object sender, EventArgs e)
        {
            var rfidConfigForm = new RFIDConfigurationForm();
            rfidConfigForm.ShowDialog();
        }

        private void GenerateRFID_Click(object sender, EventArgs e)
        {
            var rfidGeneratorForm = new RFIDGeneratorForm();
            rfidGeneratorForm.ShowDialog();
        }

        private void ConfigureSMS_Click(object sender, EventArgs e)
        {
            var smsConfigForm = new SMSConfigurationForm();
            smsConfigForm.ShowDialog();
        }

        private void ViewUpdateStudents_Click(object sender, EventArgs e)
        {
            var studentViewUpdateForm = new StudentViewUpdateForm();
            studentViewUpdateForm.ShowDialog();
        }

        private void ViewUpdateGuardians_Click(object sender, EventArgs e)
        {
            var guardianViewUpdateForm = new GuardianViewUpdateForm();
            guardianViewUpdateForm.ShowDialog();
        }

        private void LogOff_Click(object sender, EventArgs e)
        {
            this.Hide();
            var loginForm = new LoginForm();
            loginForm.FormClosed += (s, args) => this.Close();
            loginForm.Show();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timeTimer?.Stop();
            timeTimer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}