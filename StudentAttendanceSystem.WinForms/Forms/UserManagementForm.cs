using System.Security.Cryptography;
using System.Text;
using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class UserManagementForm : Form
    {
        private readonly UserRepository _userRepository;
        private readonly User _currentUser;
        private DataGridView dgvUsers;
        private GroupBox grpUserDetails;
        private TextBox txtUserId;
        private TextBox txtUsername;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private ComboBox cmbRole;
        private CheckBox chkIsActive;
        private Button btnNew;
        private Button btnSave;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnCancel;
        private Button btnClose;
        private PictureBox picUserImage;
        private Button btnBrowseImage;
        private string? _selectedImagePath;
        private bool _isEditMode = false;

        public UserManagementForm(User currentUser)
        {
            _currentUser = currentUser;
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _userRepository = new UserRepository(dbConnection);
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.MaximizeBox = false;
            // Form properties
            this.Text = "User Management";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Data Grid View
            dgvUsers = new DataGridView
            {
                Location = new Point(12, 12),
                Size = new Size(600, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;

            // Group Box for User Details
            grpUserDetails = new GroupBox
            {
                Text = "User Details",
                Location = new Point(630, 12),
                Size = new Size(350, 500)
            };

            // User Image
            picUserImage = new PictureBox
            {
                Location = new Point(125, 25),
                Size = new Size(100, 100),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            LoadDefaultUserImage();

            btnBrowseImage = new Button
            {
                Text = "Browse Image",
                Location = new Point(125, 135),
                Size = new Size(100, 25)
            };
            btnBrowseImage.Click += BtnBrowseImage_Click;

            // User ID (hidden)
            txtUserId = new TextBox
            {
                Location = new Point(120, 170),
                Size = new Size(200, 25),
                Visible = false
            };

            // Username
            var lblUsername = new Label { Text = "Username:", Location = new Point(20, 175), Size = new Size(80, 20) };
            txtUsername = new TextBox { Location = new Point(120, 173), Size = new Size(200, 25) };

            // First Name
            var lblFirstName = new Label { Text = "First Name:", Location = new Point(20, 205), Size = new Size(80, 20) };
            txtFirstName = new TextBox { Location = new Point(120, 203), Size = new Size(200, 25) };

            // Last Name
            var lblLastName = new Label { Text = "Last Name:", Location = new Point(20, 235), Size = new Size(80, 20) };
            txtLastName = new TextBox { Location = new Point(120, 233), Size = new Size(200, 25) };

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 265), Size = new Size(80, 20) };
            txtEmail = new TextBox { Location = new Point(120, 263), Size = new Size(200, 25) };

            // Password
            var lblPassword = new Label { Text = "Password:", Location = new Point(20, 295), Size = new Size(80, 20) };
            txtPassword = new TextBox { Location = new Point(120, 293), Size = new Size(200, 25), UseSystemPasswordChar = true };

            // Role
            var lblRole = new Label { Text = "Role:", Location = new Point(20, 325), Size = new Size(80, 20) };
            cmbRole = new ComboBox
            {
                Location = new Point(120, 323),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRole.Items.AddRange(new object[] { "Administrator", "Regular User" });

            // Is Active
            chkIsActive = new CheckBox
            {
                Text = "Active User",
                Location = new Point(120, 355),
                Size = new Size(100, 25),
                Checked = true
            };

            // Buttons
            btnNew = new Button { Text = "New", Location = new Point(20, 400), Size = new Size(75, 30) };
            btnSave = new Button { Text = "Save", Location = new Point(105, 400), Size = new Size(75, 30), Enabled = false };
            btnUpdate = new Button { Text = "Update", Location = new Point(190, 400), Size = new Size(75, 30), Enabled = false };
            btnDelete = new Button { Text = "Delete", Location = new Point(275, 400), Size = new Size(75, 30), Enabled = false };
            btnCancel = new Button { Text = "Cancel", Location = new Point(20, 440), Size = new Size(75, 30), Enabled = false };
            btnClose = new Button { Text = "Close", Location = new Point(275, 440), Size = new Size(75, 30) };

            // Event handlers
            btnNew.Click += BtnNew_Click;
            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnCancel.Click += BtnCancel_Click;
            btnClose.Click += BtnClose_Click;

            // Add controls to group box
            grpUserDetails.Controls.AddRange(new Control[] {
                picUserImage, btnBrowseImage, txtUserId,
                lblUsername, txtUsername, lblFirstName, txtFirstName,
                lblLastName, txtLastName, lblEmail, txtEmail,
                lblPassword, txtPassword, lblRole, cmbRole, chkIsActive,
                btnNew, btnSave, btnUpdate, btnDelete, btnCancel, btnClose
            });

            // Add controls to form
            this.Controls.AddRange(new Control[] { dgvUsers, grpUserDetails });

            this.ResumeLayout(false);
        }

        private void LoadDefaultUserImage()
        {
            try
            {
                var bitmap = new Bitmap(100, 100);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.FillEllipse(Brushes.LightGray, 0, 0, 100, 100);
                    g.DrawEllipse(Pens.DarkGray, 0, 0, 99, 99);
                    g.FillEllipse(Brushes.DarkGray, 30, 25, 40, 40);
                    g.FillEllipse(Brushes.DarkGray, 20, 60, 60, 30);
                }
                picUserImage.Image = bitmap;
            }
            catch
            {
                // Fallback if image creation fails
            }
        }

        private async Task LoadUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                var displayData = users.Select(u => new
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive ? "Yes" : "No",
                    CreatedDate = u.CreatedDate.ToString("yyyy-MM-dd")
                }).ToList();

                dgvUsers.DataSource = displayData;

                if (dgvUsers.Columns["UserId"] != null)
                    dgvUsers.Columns["UserId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow?.Index >= 0)
            {
                var userId = (int)dgvUsers.CurrentRow.Cells["UserId"].Value;
                var user = await _userRepository.GetUserByIdAsync(userId);
                
                if (user != null)
                {
                    PopulateUserDetails(user);
                    SetEditMode(false);
                }
            }
        }

        private void PopulateUserDetails(User user)
        {
            txtUserId.Text = user.UserId.ToString();
            txtUsername.Text = user.Username;
            txtFirstName.Text = user.FirstName;
            txtLastName.Text = user.LastName;
            txtEmail.Text = user.Email;
            txtPassword.Text = ""; // Don't show existing password
            cmbRole.SelectedIndex = (int)user.Role;
            chkIsActive.Checked = user.IsActive;
            _selectedImagePath = user.ImagePath;

            // Load user image if available
            if (!string.IsNullOrEmpty(user.ImagePath) && File.Exists(user.ImagePath))
            {
                try
                {
                    picUserImage.Image = Image.FromFile(user.ImagePath);
                }
                catch
                {
                    LoadDefaultUserImage();
                }
            }
            else
            {
                LoadDefaultUserImage();
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
            SetEditMode(true);
            _isEditMode = false;
            txtUsername.Focus();
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                var user = new User
                {
                    Username = txtUsername.Text.Trim(),
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    ImagePath = _selectedImagePath,
                    Role = (UserRole)cmbRole.SelectedIndex,
                    IsActive = chkIsActive.Checked
                };

                string passwordHash = HashPassword(txtPassword.Text);
                int userId = await _userRepository.CreateUserAsync(user, passwordHash);

                MessageBox.Show("User created successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadUsers();
                SetEditMode(false);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating user: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                var user = new User
                {
                    UserId = int.Parse(txtUserId.Text),
                    Username = txtUsername.Text.Trim(),
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    ImagePath = _selectedImagePath,
                    Role = (UserRole)cmbRole.SelectedIndex,
                    IsActive = chkIsActive.Checked
                };

                string? passwordHash = null;
                if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    passwordHash = HashPassword(txtPassword.Text);
                }

                await _userRepository.UpdateUserAsync(user, passwordHash);

                MessageBox.Show("User updated successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadUsers();
                SetEditMode(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserId.Text)) return;

            var userId = int.Parse(txtUserId.Text);
            
            // Prevent deleting current user
            if (userId == _currentUser.UserId)
            {
                MessageBox.Show("You cannot delete your own account!", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await _userRepository.DeleteUserAsync(userId);
                    MessageBox.Show("User deleted successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadUsers();
                    ClearForm();
                    SetEditMode(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            SetEditMode(false);
            if (dgvUsers.CurrentRow != null)
            {
                DgvUsers_SelectionChanged(dgvUsers, EventArgs.Empty);
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
            openFileDialog.Title = "Select User Image";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _selectedImagePath = openFileDialog.FileName;
                    picUserImage.Image = Image.FromFile(_selectedImagePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadDefaultUserImage();
                    _selectedImagePath = null;
                }
            }
        }

        private void SetEditMode(bool isEditing)
        {
            txtUsername.ReadOnly = !isEditing;
            txtFirstName.ReadOnly = !isEditing;
            txtLastName.ReadOnly = !isEditing;
            txtEmail.ReadOnly = !isEditing;
            txtPassword.ReadOnly = !isEditing;
            cmbRole.Enabled = isEditing;
            chkIsActive.Enabled = isEditing;
            btnBrowseImage.Enabled = isEditing;

            btnSave.Enabled = isEditing && !_isEditMode;
            btnUpdate.Enabled = isEditing && _isEditMode;
            btnCancel.Enabled = isEditing;
            btnNew.Enabled = !isEditing;
            btnDelete.Enabled = !isEditing && !string.IsNullOrEmpty(txtUserId.Text);

            if (isEditing && !string.IsNullOrEmpty(txtUserId.Text))
            {
                _isEditMode = true;
            }
        }

        private void ClearForm()
        {
            txtUserId.Text = "";
            txtUsername.Text = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtPassword.Text = "";
            cmbRole.SelectedIndex = -1;
            chkIsActive.Checked = true;
            _selectedImagePath = null;
            LoadDefaultUserImage();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
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

            if (!_isEditMode && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Password is required for new users.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            if (cmbRole.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a role.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbRole.Focus();
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

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(hashedBytes);
            }
        }
    }
}