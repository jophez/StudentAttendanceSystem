using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class RFIDGeneratorForm : Form
    {
        private readonly StudentRepository _studentRepository;
        private DataGridView dgvStudents;
        private GroupBox grpRFIDGenerator;
        private TextBox txtSelectedStudent;
        private TextBox txtCurrentRFID;
        private TextBox txtNewRFID;
        private Button btnGenerateRFID;
        private Button btnAssignRFID;
        private Button btnRemoveRFID;
        private Button btnClose;
        private Label lblInstructions;
        private int _selectedStudentId = 0;

        public RFIDGeneratorForm()
        {
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _studentRepository = new StudentRepository(dbConnection);
            InitializeComponent();
            LoadStudents();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "RFID Generator";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Instructions
            lblInstructions = new Label
            {
                Text = "Select a student from the list below to generate or manage their RFID code. Each student can only have one RFID code.",
                Location = new Point(12, 12),
                Size = new Size(960, 40),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.DarkBlue
            };

            // Data Grid View for Students
            dgvStudents = new DataGridView
            {
                Location = new Point(12, 60),
                Size = new Size(700, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            dgvStudents.SelectionChanged += DgvStudents_SelectionChanged;

            // Group Box for RFID Generator
            grpRFIDGenerator = new GroupBox
            {
                Text = "RFID Code Management",
                Location = new Point(730, 60),
                Size = new Size(250, 400)
            };

            // Selected Student
            var lblSelectedStudent = new Label { Text = "Selected Student:", Location = new Point(20, 30), Size = new Size(100, 20) };
            txtSelectedStudent = new TextBox 
            { 
                Location = new Point(20, 50), 
                Size = new Size(200, 25), 
                ReadOnly = true,
                BackColor = Color.LightGray
            };

            // Current RFID
            var lblCurrentRFID = new Label { Text = "Current RFID:", Location = new Point(20, 85), Size = new Size(100, 20) };
            txtCurrentRFID = new TextBox 
            { 
                Location = new Point(20, 105), 
                Size = new Size(200, 25), 
                ReadOnly = true,
                BackColor = Color.LightGray
            };

            // New RFID
            var lblNewRFID = new Label { Text = "New RFID Code:", Location = new Point(20, 140), Size = new Size(100, 20) };
            txtNewRFID = new TextBox 
            { 
                Location = new Point(20, 160), 
                Size = new Size(200, 25),
                ReadOnly = true,
                BackColor = Color.White
            };

            // Buttons
            btnGenerateRFID = new Button 
            { 
                Text = "Generate New RFID", 
                Location = new Point(20, 200), 
                Size = new Size(200, 35),
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnGenerateRFID.Click += BtnGenerateRFID_Click;

            btnAssignRFID = new Button 
            { 
                Text = "Assign RFID to Student", 
                Location = new Point(20, 245), 
                Size = new Size(200, 35),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Enabled = false
            };
            btnAssignRFID.Click += BtnAssignRFID_Click;

            btnRemoveRFID = new Button 
            { 
                Text = "Remove RFID", 
                Location = new Point(20, 290), 
                Size = new Size(200, 35),
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Enabled = false
            };
            btnRemoveRFID.Click += BtnRemoveRFID_Click;

            btnClose = new Button 
            { 
                Text = "Close", 
                Location = new Point(20, 335), 
                Size = new Size(200, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnClose.Click += BtnClose_Click;

            // Add controls to group box
            grpRFIDGenerator.Controls.AddRange(new Control[] {
                lblSelectedStudent, txtSelectedStudent,
                lblCurrentRFID, txtCurrentRFID,
                lblNewRFID, txtNewRFID,
                btnGenerateRFID, btnAssignRFID, btnRemoveRFID, btnClose
            });

            // Add controls to form
            this.Controls.AddRange(new Control[] { lblInstructions, dgvStudents, grpRFIDGenerator });

            this.ResumeLayout(false);
        }

        private async Task LoadStudents()
        {
            try
            {
                var students = await _studentRepository.GetAllStudentsAsync();
                var displayData = students.Where(s => s.IsActive).Select(s => new
                {
                    StudentId = s.StudentId,
                    StudentNumber = s.StudentNumber,
                    FullName = $"{s.FirstName} {s.MiddleName} {s.LastName}".Replace("  ", " ").Trim(),
                    CellPhone = s.CellPhone,
                    RFIDCode = string.IsNullOrEmpty(s.RFIDTag) ? "Not Assigned" : s.RFIDTag,
                    RFIDStatus = string.IsNullOrEmpty(s.RFIDTag) ? "No RFID" : "Has RFID"
                }).ToList();

                dgvStudents.DataSource = displayData;

                if (dgvStudents.Columns["StudentId"] != null)
                    dgvStudents.Columns["StudentId"].Visible = false;

                // Color rows based on RFID status
                foreach (DataGridViewRow row in dgvStudents.Rows)
                {
                    if (row.Cells["RFIDStatus"].Value?.ToString() == "No RFID")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow?.Index >= 0)
            {
                _selectedStudentId = (int)dgvStudents.CurrentRow.Cells["StudentId"].Value;
                var student = await _studentRepository.GetStudentByIdAsync(_selectedStudentId);
                
                if (student != null)
                {
                    UpdateSelectedStudentInfo(student);
                }
            }
        }

        private void UpdateSelectedStudentInfo(Student student)
        {
            txtSelectedStudent.Text = $"{student.FirstName} {student.MiddleName} {student.LastName}".Replace("  ", " ").Trim();
            txtCurrentRFID.Text = string.IsNullOrEmpty(student.RFIDTag) ? "No RFID assigned" : student.RFIDTag;
            
            // Update button states
            bool hasRFID = !string.IsNullOrEmpty(student.RFIDTag);
            btnRemoveRFID.Enabled = hasRFID;
            
            // Clear new RFID field when selecting different student
            txtNewRFID.Text = "";
            btnAssignRFID.Enabled = false;
        }

        private void BtnGenerateRFID_Click(object sender, EventArgs e)
        {
            if (_selectedStudentId == 0)
            {
                MessageBox.Show("Please select a student first.", "No Student Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string newRFID = GenerateUniqueRFIDCode();
                txtNewRFID.Text = newRFID;
                btnAssignRFID.Enabled = true;

                MessageBox.Show($"New RFID code generated: {newRFID}\n\nClick 'Assign RFID to Student' to save this code to the selected student.", 
                    "RFID Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating RFID: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAssignRFID_Click(object sender, EventArgs e)
        {
            if (_selectedStudentId == 0 || string.IsNullOrEmpty(txtNewRFID.Text))
            {
                MessageBox.Show("Please select a student and generate an RFID code first.", "Invalid Operation", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Check if RFID code already exists
                var existingStudent = await _studentRepository.GetStudentByRFIDAsync(txtNewRFID.Text);
                if (existingStudent != null && existingStudent.StudentId != _selectedStudentId)
                {
                    MessageBox.Show($"This RFID code is already assigned to another student: {existingStudent.FirstName} {existingStudent.LastName}", 
                        "RFID Already Assigned", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var student = await _studentRepository.GetStudentByIdAsync(_selectedStudentId);
                if (student != null)
                {
                    student.RFIDTag = txtNewRFID.Text;
                    await _studentRepository.UpdateStudentAsync(student);

                    MessageBox.Show("RFID code assigned successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh the display
                    await LoadStudents();
                    UpdateSelectedStudentInfo(student);
                    txtNewRFID.Text = "";
                    btnAssignRFID.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning RFID: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnRemoveRFID_Click(object sender, EventArgs e)
        {
            if (_selectedStudentId == 0)
            {
                MessageBox.Show("Please select a student first.", "No Student Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to remove the RFID code from this student?\n\nThis action cannot be undone.", 
                "Confirm RFID Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var student = await _studentRepository.GetStudentByIdAsync(_selectedStudentId);
                    if (student != null)
                    {
                        student.RFIDTag = null;
                        await _studentRepository.UpdateStudentAsync(student);

                        MessageBox.Show("RFID code removed successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh the display
                        await LoadStudents();
                        UpdateSelectedStudentInfo(student);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error removing RFID: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string GenerateUniqueRFIDCode()
        {
            // Generate a unique 10-digit RFID code using timestamp and random number
            var timestamp = DateTime.Now;
            var random = new Random();
            
            // Use year (last 2 digits) + month + day + hour + minute + random 2 digits
            var rfidCode = $"{timestamp:yy}{timestamp:MM}{timestamp:dd}{timestamp:HH}{timestamp:mm}{random.Next(10, 99)}";
            
            return rfidCode;
        }
    }
}