using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class StudentManagementForm : Form
    {
        private readonly StudentRepository _studentRepository;
        private readonly GuardianRepository _guardianRepository;
        private DataGridView dgvStudents;
        private GroupBox grpStudentDetails;
        private TextBox txtStudentId;
        private TextBox txtStudentNumber;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        private TextBox txtLastName;
        private TextBox txtCellPhone;
        private TextBox txtEmail;
        private TextBox txtStreetAddress;
        private TextBox txtBarangay;
        private TextBox txtMunicipality;
        private TextBox txtCity;
        private ComboBox cmbGuardian;
        private TextBox txtRFIDCode;
        private CheckBox chkIsActive;
        private Button btnNew;
        private Button btnSave;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnCancel;
        private Button btnClose;
        private PictureBox picStudentImage;
        private Button btnBrowseImage;
        private string? _selectedImagePath;
        private bool _isEditMode = false;

        public StudentManagementForm()
        {
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _studentRepository = new StudentRepository(dbConnection);
            _guardianRepository = new GuardianRepository(dbConnection);
            InitializeComponent();
            LoadStudents();
            LoadGuardians();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.MaximizeBox = false;
            // Form properties
            this.Text = "Student Management";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Data Grid View
            dgvStudents = new DataGridView
            {
                Location = new Point(12, 12),
                Size = new Size(700, 400),
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
                Text = "Student Details",
                Location = new Point(730, 12),
                Size = new Size(450, 650)
            };

            // Student Image
            picStudentImage = new PictureBox
            {
                Location = new Point(175, 25),
                Size = new Size(100, 100),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            LoadDefaultStudentImage();

            btnBrowseImage = new Button
            {
                Text = "Browse Image",
                Location = new Point(175, 135),
                Size = new Size(100, 25)
            };
            btnBrowseImage.Click += BtnBrowseImage_Click;

            // Student ID (hidden)
            txtStudentId = new TextBox
            {
                Location = new Point(150, 170),
                Size = new Size(250, 25),
                Visible = false
            };

            // Student Number
            var lblStudentNumber = new Label { Text = "Student #:", Location = new Point(20, 175), Size = new Size(120, 20) };
            txtStudentNumber = new TextBox { Location = new Point(150, 173), Size = new Size(250, 25) };

            // First Name
            var lblFirstName = new Label { Text = "First Name:", Location = new Point(20, 205), Size = new Size(120, 20) };
            txtFirstName = new TextBox { Location = new Point(150, 203), Size = new Size(250, 25) };

            // Middle Name
            var lblMiddleName = new Label { Text = "Middle Name:", Location = new Point(20, 235), Size = new Size(120, 20) };
            txtMiddleName = new TextBox { Location = new Point(150, 233), Size = new Size(250, 25) };

            // Last Name
            var lblLastName = new Label { Text = "Last Name:", Location = new Point(20, 265), Size = new Size(120, 20) };
            txtLastName = new TextBox { Location = new Point(150, 263), Size = new Size(250, 25) };

            // Cell Phone
            var lblCellPhone = new Label { Text = "Cell Phone:", Location = new Point(20, 295), Size = new Size(120, 20) };
            txtCellPhone = new TextBox { Location = new Point(150, 293), Size = new Size(250, 25) };

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 325), Size = new Size(120, 20) };
            txtEmail = new TextBox { Location = new Point(150, 323), Size = new Size(250, 25) };

            // Address fields
            var lblStreetAddress = new Label { Text = "Street Address:", Location = new Point(20, 355), Size = new Size(120, 20) };
            txtStreetAddress = new TextBox { Location = new Point(150, 353), Size = new Size(250, 25) };

            var lblBarangay = new Label { Text = "Barangay:", Location = new Point(20, 385), Size = new Size(120, 20) };
            txtBarangay = new TextBox { Location = new Point(150, 383), Size = new Size(250, 25) };

            var lblMunicipality = new Label { Text = "Municipality:", Location = new Point(20, 415), Size = new Size(120, 20) };
            txtMunicipality = new TextBox { Location = new Point(150, 413), Size = new Size(250, 25) };

            var lblCity = new Label { Text = "City:", Location = new Point(20, 445), Size = new Size(120, 20) };
            txtCity = new TextBox { Location = new Point(150, 443), Size = new Size(250, 25) };

            // Guardian
            var lblGuardian = new Label { Text = "Guardian:", Location = new Point(20, 475), Size = new Size(120, 20) };
            cmbGuardian = new ComboBox
            {
                Location = new Point(150, 473),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // RFID Code
            var lblRFIDCode = new Label { Text = "RFID Code:", Location = new Point(20, 505), Size = new Size(120, 20) };
            txtRFIDCode = new TextBox { Location = new Point(150, 503), Size = new Size(200, 25), ReadOnly = true };
            var btnGenerateRFID = new Button
            {
                Text = "Generate",
                Location = new Point(360, 503),
                Size = new Size(70, 25)
            };
            btnGenerateRFID.Click += BtnGenerateRFID_Click;

            // Is Active
            chkIsActive = new CheckBox
            {
                Text = "Active Student",
                Location = new Point(150, 535),
                Size = new Size(120, 25),
                Checked = true
            };

            // Buttons
            btnNew = new Button { Text = "New", Location = new Point(20, 580), Size = new Size(75, 30) };
            btnSave = new Button { Text = "Save", Location = new Point(105, 580), Size = new Size(75, 30), Enabled = false };
            btnUpdate = new Button { Text = "Edit", Location = new Point(190, 580), Size = new Size(75, 30), Enabled = false };
            btnDelete = new Button { Text = "Delete", Location = new Point(275, 580), Size = new Size(75, 30), Enabled = false };
            btnCancel = new Button { Text = "Cancel", Location = new Point(20, 620), Size = new Size(75, 30), Enabled = false };
            btnClose = new Button { Text = "Close", Location = new Point(355, 620), Size = new Size(75, 30) };

            // Event handlers
            btnNew.Click += BtnNew_Click;
            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnCancel.Click += BtnCancel_Click;
            btnClose.Click += BtnClose_Click;

            // Add controls to group box
            grpStudentDetails.Controls.AddRange(new Control[] {
                picStudentImage, btnBrowseImage, txtStudentId,
                lblStudentNumber, txtStudentNumber, lblFirstName, txtFirstName,
                lblMiddleName, txtMiddleName, lblLastName, txtLastName,
                lblCellPhone, txtCellPhone, lblEmail, txtEmail,
                lblStreetAddress, txtStreetAddress, lblBarangay, txtBarangay,
                lblMunicipality, txtMunicipality, lblCity, txtCity,
                lblGuardian, cmbGuardian, lblRFIDCode, txtRFIDCode, btnGenerateRFID,
                chkIsActive, btnNew, btnSave, btnUpdate, btnDelete, btnCancel, btnClose
            });

            // Add controls to form
            this.Controls.AddRange(new Control[] { dgvStudents, grpStudentDetails });

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
                var displayData = students.Select(s => new
                {
                    StudentId = s.StudentId,
                    StudentNumber = s.StudentNumber,
                    FullName = $"{s.FirstName} {s.MiddleName} {s.LastName}".Replace("  ", " ").Trim(),
                    CellPhone = s.CellPhone,
                    Email = s.Email,
                    Guardian = s.Guardian != null ? $"{s.Guardian.FirstName} {s.Guardian.LastName}" : "",
                    RFIDCode = s.RFIDTag ?? "Not Assigned",
                    IsActive = s.IsActive ? "Yes" : "No"
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

        private async void LoadGuardians()
        {
            try
            {
                var guardians = await _guardianRepository.GetAllGuardiansAsync();
                cmbGuardian.Items.Clear();
                cmbGuardian.Items.Add(new { GuardianId = (int?)null, DisplayName = "-- No Guardian --" });

                foreach (var guardian in guardians)
                {
                    cmbGuardian.Items.Add(new
                    {
                        GuardianId = (int?)guardian.GuardianId,
                        DisplayName = $"{guardian.FirstName} {guardian.LastName}"
                    });
                }

                cmbGuardian.DisplayMember = "DisplayName";
                cmbGuardian.ValueMember = "GuardianId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading guardians: {ex.Message}", "Error",
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
                    SetEditMode(false);
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
            txtStreetAddress.Text = student.StreetAddress;
            txtBarangay.Text = student.Barangay;
            txtMunicipality.Text = student.Municipality;
            txtCity.Text = student.City;
            txtRFIDCode.Text = student.RFIDTag ?? "";
            chkIsActive.Checked = student.IsActive;
            _selectedImagePath = student.ImagePath;

            // Set guardian selection
            if (student.GuardianId.HasValue)
            {
                for (int i = 0; i < cmbGuardian.Items.Count; i++)
                {
                    var item = (dynamic)cmbGuardian.Items[i];
                    if (item.GuardianId == student.GuardianId)
                    {
                        cmbGuardian.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                cmbGuardian.SelectedIndex = 0; // No Guardian
            }

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
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
            SetEditMode(true);
            _isEditMode = false;
            btnSave.Text = "Save";
            txtStudentNumber.Focus();
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                var student = new Student
                {
                    StudentNumber = txtStudentNumber.Text.Trim(),
                    FirstName = txtFirstName.Text.Trim(),
                    MiddleName = txtMiddleName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    CellPhone = txtCellPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    ImagePath = _selectedImagePath,
                    StreetAddress = txtStreetAddress.Text.Trim(),
                    Barangay = txtBarangay.Text.Trim(),
                    Municipality = txtMunicipality.Text.Trim(),
                    City = txtCity.Text.Trim(),
                    GuardianId = GetSelectedGuardianId(),
                    RFIDTag = string.IsNullOrWhiteSpace(txtRFIDCode.Text) ? null : txtRFIDCode.Text.Trim(),
                    IsActive = chkIsActive.Checked
                };

                if (_isEditMode)
                {
                    // Update existing student
                    student.StudentId = int.Parse(txtStudentId.Text);

                    // Use the updated UpdateStudentAsync method with tuple return
                    var success = await _studentRepository.UpdateStudentAsync(student);

                    if (success)
                    {
                        MessageBox.Show("Student information updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadStudents();
                        SetEditMode(false);
                        _isEditMode = false;
                        btnSave.Text = "Save";

                        // Stay on the updated record
                        DgvStudents_SelectionChanged(dgvStudents, EventArgs.Empty);
                    }
                    else
                    {
                        MessageBox.Show("Student information update failed!", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // Create new student
                    var studentId = await _studentRepository.CreateStudentAsync(student);
                    MessageBox.Show("Student created successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadStudents();
                    SetEditMode(false);
                    _isEditMode = false;
                    btnSave.Text = "Save";
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                var operation = _isEditMode ? "updating" : "creating";
                MessageBox.Show($"Error {operation} student: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Check if we have a student selected
            if (string.IsNullOrEmpty(txtStudentId.Text))
            {
                MessageBox.Show("Please select a student to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Switch to edit mode
            SetEditMode(true);
            _isEditMode = true;
            btnSave.Text = "Update";
            txtStudentNumber.Focus(); // Set focus to first editable field
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStudentId.Text)) return;

            var result = MessageBox.Show("Are you sure you want to delete this student?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var studentId = int.Parse(txtStudentId.Text);
                    await _studentRepository.DeleteStudentAsync(studentId);

                    MessageBox.Show("Student deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadStudents();
                    ClearForm();
                    SetEditMode(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting student: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            SetEditMode(false);
            _isEditMode = false;
            btnSave.Text = "Save";

            // If we have a selected student, reload their data
            if (dgvStudents.CurrentRow != null)
            {
                DgvStudents_SelectionChanged(dgvStudents, EventArgs.Empty);
            }
            else
            {
                ClearForm();
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            openFileDialog.Title = "Select Student Image";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _selectedImagePath = openFileDialog.FileName;
                    picStudentImage.Image = Image.FromFile(_selectedImagePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadDefaultStudentImage();
                    _selectedImagePath = null;
                }
            }
        }

        private void BtnGenerateRFID_Click(object sender, EventArgs e)
        {
            // Generate a unique RFID code
            var rfidCode = GenerateRFIDCode();
            txtRFIDCode.Text = rfidCode;
        }

        private string GenerateRFIDCode()
        {
            // Generate a 10-digit unique RFID code
            var random = new Random();
            var timestamp = DateTime.Now.Ticks.ToString();
            var randomPart = random.Next(1000, 9999).ToString();
            return timestamp.Substring(timestamp.Length - 6) + randomPart;
        }

        private int? GetSelectedGuardianId()
        {
            if (cmbGuardian.SelectedItem != null)
            {
                var selectedItem = (dynamic)cmbGuardian.SelectedItem;
                return selectedItem.GuardianId;
            }
            return null;
        }

        private void SetEditMode(bool isEditing)
        {
            // Enable/disable form controls based on editing state
            txtStudentNumber.ReadOnly = !isEditing;
            txtFirstName.ReadOnly = !isEditing;
            txtMiddleName.ReadOnly = !isEditing;
            txtLastName.ReadOnly = !isEditing;
            txtCellPhone.ReadOnly = !isEditing;
            txtEmail.ReadOnly = !isEditing;
            txtStreetAddress.ReadOnly = !isEditing;
            txtBarangay.ReadOnly = !isEditing;
            txtMunicipality.ReadOnly = !isEditing;
            txtCity.ReadOnly = !isEditing;
            cmbGuardian.Enabled = isEditing;
            chkIsActive.Enabled = isEditing;
            txtRFIDCode.ReadOnly = !isEditing;

            // Image browse button should be enabled during create/update only
            btnBrowseImage.Enabled = isEditing;

            // Button states
            btnSave.Enabled = isEditing;
            btnCancel.Enabled = isEditing;
            btnNew.Enabled = !isEditing;

            // Enable Update and Delete buttons only when viewing (not editing) and a student is selected
            bool hasSelectedStudent = !string.IsNullOrEmpty(txtStudentId.Text);
            btnUpdate.Enabled = !isEditing && hasSelectedStudent;
            btnDelete.Enabled = !isEditing && hasSelectedStudent;
        }

        private void ClearForm()
        {
            txtStudentId.Text = "";
            txtStudentNumber.Text = "";
            txtFirstName.Text = "";
            txtMiddleName.Text = "";
            txtLastName.Text = "";
            txtCellPhone.Text = "";
            txtEmail.Text = "";
            txtStreetAddress.Text = "";
            txtBarangay.Text = "";
            txtMunicipality.Text = "";
            txtCity.Text = "";
            txtRFIDCode.Text = "";
            cmbGuardian.SelectedIndex = 0;
            chkIsActive.Checked = true;
            _selectedImagePath = null;
            LoadDefaultStudentImage();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtStudentNumber.Text))
            {
                MessageBox.Show("Student number is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStudentNumber.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("First name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Last name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return false;
            }

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