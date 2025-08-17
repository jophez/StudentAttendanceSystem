using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class GuardianManagementForm : Form
    {
        private readonly GuardianRepository _guardianRepository;
        private DataGridView dgvGuardians;
        private GroupBox grpGuardianDetails;
        private TextBox txtGuardianId;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtCellPhone;
        private TextBox txtEmail;
        private TextBox txtRelationship;
        private CheckBox chkIsActive;
        private Button btnNew;
        private Button btnSave;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnCancel;
        private Button btnClose;
        private bool _isEditMode = false;

        public GuardianManagementForm()
        {
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _guardianRepository = new GuardianRepository(dbConnection);
            InitializeComponent();
            LoadGuardians();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Guardian Management";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Data Grid View
            dgvGuardians = new DataGridView
            {
                Location = new Point(12, 12),
                Size = new Size(650, 400),
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
                Text = "Guardian Details",
                Location = new Point(680, 12),
                Size = new Size(300, 450)
            };

            // Guardian ID (hidden)
            txtGuardianId = new TextBox
            {
                Location = new Point(120, 30),
                Size = new Size(150, 25),
                Visible = false
            };

            // First Name
            var lblFirstName = new Label { Text = "First Name:", Location = new Point(20, 35), Size = new Size(80, 20) };
            txtFirstName = new TextBox { Location = new Point(120, 33), Size = new Size(150, 25) };

            // Last Name
            var lblLastName = new Label { Text = "Last Name:", Location = new Point(20, 65), Size = new Size(80, 20) };
            txtLastName = new TextBox { Location = new Point(120, 63), Size = new Size(150, 25) };

            // Cell Phone
            var lblCellPhone = new Label { Text = "Cell Phone:", Location = new Point(20, 95), Size = new Size(80, 20) };
            txtCellPhone = new TextBox { Location = new Point(120, 93), Size = new Size(150, 25) };

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 125), Size = new Size(80, 20) };
            txtEmail = new TextBox { Location = new Point(120, 123), Size = new Size(150, 25) };

            // Relationship
            var lblRelationship = new Label { Text = "Relationship:", Location = new Point(20, 155), Size = new Size(80, 20) };
            txtRelationship = new TextBox { Location = new Point(120, 153), Size = new Size(150, 25) };

            // Is Active
            chkIsActive = new CheckBox
            {
                Text = "Active Guardian",
                Location = new Point(120, 185),
                Size = new Size(120, 25),
                Checked = true
            };

            // Buttons
            btnNew = new Button { Text = "New", Location = new Point(20, 230), Size = new Size(75, 30) };
            btnSave = new Button { Text = "Save", Location = new Point(105, 230), Size = new Size(75, 30), Enabled = false };
            btnUpdate = new Button { Text = "Update", Location = new Point(190, 230), Size = new Size(75, 30), Enabled = false };
            btnDelete = new Button { Text = "Delete", Location = new Point(20, 270), Size = new Size(75, 30), Enabled = false };
            btnCancel = new Button { Text = "Cancel", Location = new Point(105, 270), Size = new Size(75, 30), Enabled = false };
            btnClose = new Button { Text = "Close", Location = new Point(190, 270), Size = new Size(75, 30) };

            // Event handlers
            btnNew.Click += BtnNew_Click;
            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnCancel.Click += BtnCancel_Click;
            btnClose.Click += BtnClose_Click;

            // Add controls to group box
            grpGuardianDetails.Controls.AddRange(new Control[] {
                txtGuardianId, lblFirstName, txtFirstName, lblLastName, txtLastName,
                lblCellPhone, txtCellPhone, lblEmail, txtEmail,
                lblRelationship, txtRelationship, chkIsActive,
                btnNew, btnSave, btnUpdate, btnDelete, btnCancel, btnClose
            });

            // Add controls to form
            this.Controls.AddRange(new Control[] { dgvGuardians, grpGuardianDetails });

            this.ResumeLayout(false);
        }

        private async Task LoadGuardians()
        {
            try
            {
                var guardians = await _guardianRepository.GetAllGuardiansAsync();
                var displayData = guardians.Select(g => new
                {
                    GuardianId = g.GuardianId,
                    FullName = $"{g.FirstName} {g.LastName}",
                    CellPhone = g.CellPhone,
                    Email = g.Email,
                    //Relationship = g.Relationship,
                    IsActive = g.IsActive ? "Yes" : "No",
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
                    SetEditMode(false);
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
            chkIsActive.Checked = guardian.IsActive;
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
            SetEditMode(true);
            _isEditMode = false;
            txtFirstName.Focus();
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                var guardian = new Guardian
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    CellPhone = txtCellPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                   // Relationship = txtRelationship.Text.Trim(),
                    IsActive = chkIsActive.Checked
                };

                int guardianId = await _guardianRepository.CreateGuardianAsync(guardian);

                MessageBox.Show("Guardian created successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadGuardians();
                SetEditMode(false);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating guardian: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                var guardian = new Guardian
                {
                    GuardianId = int.Parse(txtGuardianId.Text),
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    CellPhone = txtCellPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    //Relationship = txtRelationship.Text.Trim(),
                    IsActive = chkIsActive.Checked
                };

                await _guardianRepository.UpdateGuardianAsync(guardian);

                MessageBox.Show("Guardian updated successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadGuardians();
                SetEditMode(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating guardian: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtGuardianId.Text)) return;

            var result = MessageBox.Show("Are you sure you want to delete this guardian?\n\nNote: This will also remove the guardian assignment from any associated students.", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var guardianId = int.Parse(txtGuardianId.Text);
                    await _guardianRepository.DeleteGuardianAsync(guardianId);
                    
                    MessageBox.Show("Guardian deleted successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadGuardians();
                    ClearForm();
                    SetEditMode(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting guardian: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            SetEditMode(false);
            if (dgvGuardians.CurrentRow != null)
            {
                DgvGuardians_SelectionChanged(dgvGuardians, EventArgs.Empty);
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

        private void SetEditMode(bool isEditing)
        {
            txtFirstName.ReadOnly = !isEditing;
            txtLastName.ReadOnly = !isEditing;
            txtCellPhone.ReadOnly = !isEditing;
            txtEmail.ReadOnly = !isEditing;
            txtRelationship.ReadOnly = !isEditing;
            chkIsActive.Enabled = isEditing;

            btnSave.Enabled = isEditing && !_isEditMode;
            btnUpdate.Enabled = isEditing && _isEditMode;
            btnCancel.Enabled = isEditing;
            btnNew.Enabled = !isEditing;
            btnDelete.Enabled = !isEditing && !string.IsNullOrEmpty(txtGuardianId.Text);

            if (isEditing && !string.IsNullOrEmpty(txtGuardianId.Text))
            {
                _isEditMode = true;
            }
        }

        private void ClearForm()
        {
            txtGuardianId.Text = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtCellPhone.Text = "";
            txtEmail.Text = "";
            txtRelationship.Text = "";
            chkIsActive.Checked = true;
        }

        private bool ValidateInput()
        {
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

            if (string.IsNullOrWhiteSpace(txtRelationship.Text))
            {
                MessageBox.Show("Relationship is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRelationship.Focus();
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