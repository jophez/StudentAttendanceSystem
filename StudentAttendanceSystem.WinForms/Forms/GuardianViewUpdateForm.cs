using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class GuardianViewUpdateForm : Form
    {
        private readonly GuardianRepository _guardianRepository;
        private readonly StudentRepository _studentRepository;
        private DataGridView dgvGuardians;
        private GroupBox grpGuardianDetails;
        private TextBox txtGuardianId;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtCellPhone;
        private TextBox txtEmail;
        private TextBox txtRelationship;
        private DataGridView dgvAssociatedStudents;
        private Button btnUpdate;
        private Button btnCancel;
        private Button btnClose;
        private bool _isEditMode = false;

        public GuardianViewUpdateForm()
        {
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _guardianRepository = new GuardianRepository(dbConnection);
            _studentRepository = new StudentRepository(dbConnection);
            InitializeComponent();
            LoadGuardians();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Guardian Information - View/Update";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Data Grid View for Guardians
            dgvGuardians = new DataGridView
            {
                Location = new Point(12, 12),
                Size = new Size(650, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            dgvGuardians.SelectionChanged += DgvGuardians_SelectionChanged;

            // Group Box for Guardian Details
            grpGuardianDetails = new GroupBox
            {
                Text = "Guardian Details (Limited Access)",
                Location = new Point(680, 12),
                Size = new Size(300, 300)
            };

            // Guardian ID (hidden)
            txtGuardianId = new TextBox
            {
                Location = new Point(120, 30),
                Size = new Size(150, 25),
                Visible = false
            };

            // First Name (Read-only)
            var lblFirstName = new Label { Text = "First Name:", Location = new Point(20, 35), Size = new Size(80, 20) };
            txtFirstName = new TextBox 
            { 
                Location = new Point(120, 33), 
                Size = new Size(150, 25), 
                ReadOnly = true, 
                BackColor = Color.LightGray 
            };

            // Last Name (Read-only)
            var lblLastName = new Label { Text = "Last Name:", Location = new Point(20, 65), Size = new Size(80, 20) };
            txtLastName = new TextBox 
            { 
                Location = new Point(120, 63), 
                Size = new Size(150, 25), 
                ReadOnly = true, 
                BackColor = Color.LightGray 
            };

            // Cell Phone (Editable)
            var lblCellPhone = new Label { Text = "Cell Phone:", Location = new Point(20, 95), Size = new Size(80, 20) };
            txtCellPhone = new TextBox { Location = new Point(120, 93), Size = new Size(150, 25) };

            // Email (Editable)
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 125), Size = new Size(80, 20) };
            txtEmail = new TextBox { Location = new Point(120, 123), Size = new Size(150, 25) };

            // Relationship (Read-only)
            var lblRelationship = new Label { Text = "Relationship:", Location = new Point(20, 155), Size = new Size(80, 20) };
            txtRelationship = new TextBox 
            { 
                Location = new Point(120, 153), 
                Size = new Size(150, 25), 
                ReadOnly = true, 
                BackColor = Color.LightGray 
            };

            // Note about limited access
            var lblNote = new Label
            {
                Text = "Note: You can only update contact information.\nOther changes require administrator approval.",
                Location = new Point(20, 185),
                Size = new Size(250, 40),
                ForeColor = Color.DarkRed,
                Font = new Font("Arial", 8, FontStyle.Italic)
            };

            // Buttons
            btnUpdate = new Button 
            { 
                Text = "Update Contact Info", 
                Location = new Point(20, 235), 
                Size = new Size(130, 30), 
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Enabled = false
            };
            btnUpdate.Click += BtnUpdate_Click;

            btnCancel = new Button 
            { 
                Text = "Cancel", 
                Location = new Point(160, 235), 
                Size = new Size(70, 30),
                Enabled = false
            };
            btnCancel.Click += BtnCancel_Click;

            btnClose = new Button 
            { 
                Text = "Close", 
                Location = new Point(240, 235), 
                Size = new Size(70, 30) 
            };
            btnClose.Click += BtnClose_Click;

            // Add controls to group box
            grpGuardianDetails.Controls.AddRange(new Control[] {
                txtGuardianId, lblFirstName, txtFirstName, lblLastName, txtLastName,
                lblCellPhone, txtCellPhone, lblEmail, txtEmail,
                lblRelationship, txtRelationship, lblNote,
                btnUpdate, btnCancel, btnClose
            });

            // Associated Students Grid
            var lblStudents = new Label
            {
                Text = "Associated Students:",
                Location = new Point(12, 330),
                Size = new Size(200, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            dgvAssociatedStudents = new DataGridView
            {
                Location = new Point(12, 355),
                Size = new Size(650, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] { dgvGuardians, grpGuardianDetails, lblStudents, dgvAssociatedStudents });

            // Enable editing when text changes
            txtCellPhone.TextChanged += (s, e) => EnableUpdateButton();
            txtEmail.TextChanged += (s, e) => EnableUpdateButton();

            this.ResumeLayout(false);
        }

        private async Task LoadGuardians()
        {
            try
            {
                var guardians = await _guardianRepository.GetAllGuardiansAsync();
                var displayData = guardians.Where(g => g.IsActive).Select(g => new
                {
                    GuardianId = g.GuardianId,
                    FullName = $"{g.FirstName} {g.LastName}",
                    CellPhone = g.CellPhone,
                    Email = g.Email,
                    //Relationship = g.Relationship,
                    CreatedDate = g.CreatedDate.ToString("yyyy-MM-dd")
                }).ToList();

                dgvGuardians.DataSource = displayData;

                if (dgvGuardians.Columns["GuardianId"] != null)
                    dgvGuardians.Columns["GuardianId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading guardians: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DgvGuardians_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvGuardians.CurrentRow?.Index >= 0)
            {
                var guardianId = (int)dgvGuardians.CurrentRow.Cells["GuardianId"].Value;
                var guardian = await _guardianRepository.GetGuardianByIdAsync(guardianId);
                
                if (guardian != null)
                {
                    PopulateGuardianDetails(guardian);
                    await LoadAssociatedStudents(guardianId);
                }
            }
        }

        private void PopulateGuardianDetails(Guardian guardian)
        {
            txtGuardianId.Text = guardian.GuardianId.ToString();
            txtFirstName.Text = guardian.FirstName;
            txtLastName.Text = guardian.LastName;
            txtCellPhone.Text = guardian.CellPhone;
            txtEmail.Text = guardian.Email;
            //txtRelationship.Text = guardian.Relationship;

            // Reset edit mode
            _isEditMode = false;
            btnUpdate.Enabled = false;
            btnCancel.Enabled = false;
        }

        private async Task LoadAssociatedStudents(int guardianId)
        {
            try
            {
                var students = await _studentRepository.GetStudentsByGuardianIdAsync(guardianId);
                var displayData = students.Select(s => new
                {
                    StudentNumber = s.StudentNumber,
                    FullName = $"{s.FirstName} {s.MiddleName} {s.LastName}".Replace("  ", " ").Trim(),
                    CellPhone = s.CellPhone,
                    Email = s.Email,
                    IsActive = s.IsActive ? "Yes" : "No"
                }).ToList();

                dgvAssociatedStudents.DataSource = displayData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading associated students: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnableUpdateButton()
        {
            if (!string.IsNullOrEmpty(txtGuardianId.Text))
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
                var guardianId = int.Parse(txtGuardianId.Text);
                var guardian = await _guardianRepository.GetGuardianByIdAsync(guardianId);
                
                if (guardian != null)
                {
                    // Only update contact information
                    guardian.CellPhone = txtCellPhone.Text.Trim();
                    guardian.Email = txtEmail.Text.Trim();

                    await _guardianRepository.UpdateGuardianAsync(guardian);

                    MessageBox.Show("Guardian contact information updated successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadGuardians();
                    _isEditMode = false;
                    btnUpdate.Enabled = false;
                    btnCancel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating guardian: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvGuardians.CurrentRow != null)
            {
                DgvGuardians_SelectionChanged(dgvGuardians, EventArgs.Empty);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool ValidateContactInfo()
        {
            if (string.IsNullOrWhiteSpace(txtCellPhone.Text))
            {
                MessageBox.Show("Cell phone number is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCellPhone.Focus();
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