using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class StudentViewUpdateForm : Form
    {
        private readonly StudentRepository _studentRepository;
        private readonly AttendanceRepository _attendanceRepository;
        private DataGridView dgvStudents;
        private GroupBox grpStudentDetails;
        private TextBox txtStudentId;
        private TextBox txtStudentNumber;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        private TextBox txtLastName;
        private TextBox txtCellPhone;
        private TextBox txtEmail;
        private DataGridView dgvAttendance;
        private Button btnUpdate;
        private Button btnCancel;
        private Button btnClose;
        private PictureBox picStudentImage;
        private bool _isEditMode = false;

        public StudentViewUpdateForm()
        {
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _studentRepository = new StudentRepository(dbConnection);
            _attendanceRepository = new AttendanceRepository(dbConnection);
            InitializeComponent();
            LoadStudents();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Student Information - View/Update";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Data Grid View for Students
            dgvStudents = new DataGridView
            {
                Location = new Point(12, 12),
                Size = new Size(600, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            dgvStudents.SelectionChanged += DgvStudents_SelectionChanged;

            // Group Box for Student Details
            grpStudentDetails = new GroupBox
            {
                Text = "Student Details (Limited Access)",
                Location = new Point(630, 12),
                Size = new Size(350, 400),
            };

            // Student Image
            picStudentImage = new PictureBox
            {
                Location = new Point(125, 25),
                Size = new Size(100, 100),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            LoadDefaultStudentImage();

            // Student ID (hidden)
            txtStudentId = new TextBox
            {
                Location = new Point(150, 135),
                Size = new Size(170, 25),
                Visible = false
            };

            // Student Number (Read-only)
            var lblStudentNumber = new Label { Text = "Student #:", Location = new Point(20, 140), Size = new Size(120, 20) };
            txtStudentNumber = new TextBox 
            { 
                Location = new Point(150, 138), 
                Size = new Size(170, 25), 
                ReadOnly = true, 
                BackColor = Color.LightGray 
            };

            // First Name (Read-only)
            var lblFirstName = new Label { Text = "First Name:", Location = new Point(20, 170), Size = new Size(120, 20) };
            txtFirstName = new TextBox 
            { 
                Location = new Point(150, 168), 
                Size = new Size(170, 25), 
                ReadOnly = true, 
                BackColor = Color.LightGray 
            };

            // Middle Name (Read-only)
            var lblMiddleName = new Label { Text = "Middle Name:", Location = new Point(20, 200), Size = new Size(120, 20) };
            txtMiddleName = new TextBox 
            { 
                Location = new Point(150, 198), 
                Size = new Size(170, 25), 
                ReadOnly = true, 
                BackColor = Color.LightGray 
            };

            // Last Name (Read-only)
            var lblLastName = new Label { Text = "Last Name:", Location = new Point(20, 230), Size = new Size(120, 20) };
            txtLastName = new TextBox 
            { 
                Location = new Point(150, 228), 
                Size = new Size(170, 25), 
                ReadOnly = true, 
                BackColor = Color.LightGray 
            };

            // Cell Phone (Editable)
            var lblCellPhone = new Label { Text = "Cell Phone:", Location = new Point(20, 260), Size = new Size(120, 20) }; 
            txtCellPhone = new TextBox { Location = new Point(150, 258), Size = new Size(170, 25) };

            // Email (Editable)
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 290), Size = new Size(120, 20) };
            txtEmail = new TextBox { Location = new Point(150, 288), Size = new Size(170, 25) };

            // Note about limited access
            var lblNote = new Label
            {
                Text = "Note: You can only update contact information.\nOther changes require administrator approval.",
                Location = new Point(20, 320),
                Size = new Size(310, 40),
                ForeColor = Color.DarkRed,
                Font = new Font("Arial", 8, FontStyle.Italic)
            };

            // Buttons
            btnUpdate = new Button 
            { 
                Text = "Update Contact Info", 
                Location = new Point(20, 365), 
                Size = new Size(130, 30), 
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Enabled = false
            };
            btnUpdate.Click += BtnUpdate_Click;

            btnCancel = new Button 
            { 
                Text = "Cancel", 
                Location = new Point(160, 365), 
                Size = new Size(70, 30),
                Enabled = false
            };
            btnCancel.Click += BtnCancel_Click;

            btnClose = new Button 
            { 
                Text = "Close", 
                Location = new Point(240, 365), 
                Size = new Size(70, 30) 
            };
            btnClose.Click += BtnClose_Click;

            // Add controls to group box
            grpStudentDetails.Controls.AddRange(new Control[] {
                picStudentImage, txtStudentId,
                lblStudentNumber, txtStudentNumber, lblFirstName, txtFirstName,
                lblMiddleName, txtMiddleName, lblLastName, txtLastName,
                lblCellPhone, txtCellPhone, lblEmail, txtEmail,
                lblNote, btnUpdate, btnCancel, btnClose
            });

            // Attendance Grid
            var lblAttendance = new Label
            {
                Text = "Recent Attendance Records:",
                Location = new Point(12, 330),
                Size = new Size(200, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            dgvAttendance = new DataGridView
            {
                Location = new Point(12, 355),
                Size = new Size(600, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] { dgvStudents, grpStudentDetails, lblAttendance, dgvAttendance });

            // Enable editing when text changes
            txtCellPhone.TextChanged += (s, e) => EnableUpdateButton();
            txtEmail.TextChanged += (s, e) => EnableUpdateButton();

            this.ResumeLayout(false);
        }

        private void LoadDefaultStudentImage()
        {
            try
            {
                var bitmap = new Bitmap(100, 100);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.FillEllipse(Brushes.LightBlue, 0, 0, 100, 100);
                    g.DrawEllipse(Pens.DarkBlue, 0, 0, 99, 99);
                    g.FillEllipse(Brushes.DarkBlue, 30, 25, 40, 40);
                    g.FillEllipse(Brushes.DarkBlue, 20, 60, 60, 30);
                }
                picStudentImage.Image = bitmap;
            }
            catch
            {
                // Fallback if image creation fails
            }
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
                    Email = s.Email,
                    Guardian = s.Guardian != null ? $"{s.Guardian.FirstName} {s.Guardian.LastName}" : "No Guardian"
                }).ToList();

                dgvStudents.DataSource = displayData;

                if (dgvStudents.Columns["StudentId"] != null)
                    dgvStudents.Columns["StudentId"].Visible = false;
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
                var studentId = (int)dgvStudents.CurrentRow.Cells["StudentId"].Value;
                var student = await _studentRepository.GetStudentByIdAsync(studentId);
                
                if (student != null)
                {
                    PopulateStudentDetails(student);
                    await LoadAttendanceRecords(studentId);
                }
            }
        }

        private void PopulateStudentDetails(Student student)
        {
            txtStudentId.Text = student.StudentId.ToString();
            txtStudentNumber.Text = student.StudentNumber;
            txtFirstName.Text = student.FirstName;
            txtMiddleName.Text = student.MiddleName;
            txtLastName.Text = student.LastName;
            txtCellPhone.Text = student.CellPhone;
            txtEmail.Text = student.Email;

            // Load student image if available
            if (!string.IsNullOrEmpty(student.ImagePath) && File.Exists(student.ImagePath))
            {
                try
                {
                    picStudentImage.Image = Image.FromFile(student.ImagePath);
                }
                catch
                {
                    LoadDefaultStudentImage();
                }
            }
            else
            {
                LoadDefaultStudentImage();
            }

            // Reset edit mode
            _isEditMode = false;
            btnUpdate.Enabled = false;
            btnCancel.Enabled = false;
        }

        private async Task LoadAttendanceRecords(int studentId)
        {
            try
            {
                var attendanceRecords = await _attendanceRepository.GetAttendanceByStudentIdAsync(studentId);
                var displayData = attendanceRecords.OrderByDescending(a => a.TimeIn).Take(20).Select(a => new
                {
                    Date = a.TimeIn.ToString() ?? "Unknown",
                    TimeIn = a.TimeIn.ToString() ?? "Not recorded",
                    TimeOut = a.TimeOut?.ToString("HH:mm:ss") ?? "Not recorded",
                    Status = a.TimeOut.HasValue ? "Complete" : "Time In Only"
                }).ToList();

                dgvAttendance.DataSource = displayData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance records: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnableUpdateButton()
        {
            if (!string.IsNullOrEmpty(txtStudentId.Text))
            {
                _isEditMode = true;
                btnUpdate.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateContactInfo()) return;

            try
            {
                var studentId = int.Parse(txtStudentId.Text);
                var student = await _studentRepository.GetStudentByIdAsync(studentId);
                
                if (student != null)
                {
                    // Only update contact information
                    student.CellPhone = txtCellPhone.Text.Trim();
                    student.Email = txtEmail.Text.Trim();

                    await _studentRepository.UpdateStudentAsync(student);

                    MessageBox.Show("Student contact information updated successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadStudents();
                    _isEditMode = false;
                    btnUpdate.Enabled = false;
                    btnCancel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating student: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow != null)
            {
                DgvStudents_SelectionChanged(dgvStudents, EventArgs.Empty);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool ValidateContactInfo()
        {
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}