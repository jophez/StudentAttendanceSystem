using System.Security.Cryptography;
using System.Text;
using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class LoginForm : Form
    {
        private readonly UserRepository _userRepository;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnExit;
        private PictureBox picUserImage;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;

        public LoginForm()
        {
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _userRepository = new UserRepository(dbConnection);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "Student Attendance System - Login";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(400, 350);
            this.BackColor = Color.White;

            // Title Label
            lblTitle = new Label
            {
                Text = "Student Attendance System",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(50, 20),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // User Image
            picUserImage = new PictureBox
            {
                Location = new Point(175, 60),
                Size = new Size(50, 50),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle
            };
            LoadDefaultUserImage();

            // Username Label
            lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(50, 130),
                Size = new Size(80, 20),
                Font = new Font("Arial", 10)
            };

            // Username TextBox
            txtUsername = new TextBox
            {
                Location = new Point(140, 128),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10)
            };

            // Password Label
            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(50, 170),
                Size = new Size(80, 20),
                Font = new Font("Arial", 10)
            };

            // Password TextBox
            txtPassword = new TextBox
            {
                Location = new Point(140, 168),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10),
                UseSystemPasswordChar = true
            };

            // Login Button
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(140, 220),
                Size = new Size(80, 35),
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.Click += BtnLogin_Click;

            // Exit Button
            btnExit = new Button
            {
                Text = "Exit",
                Location = new Point(230, 220),
                Size = new Size(80, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnExit.Click += BtnExit_Click;

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblTitle, picUserImage, lblUsername, txtUsername,
                lblPassword, txtPassword, btnLogin, btnExit
            });

            // Set default button and accept/cancel buttons
            this.AcceptButton = btnLogin;
            this.CancelButton = btnExit;

            this.ResumeLayout(false);
        }

        private void LoadDefaultUserImage()
        {
            try
            {
                // Create a simple default user icon
                var bitmap = new Bitmap(50, 50);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.FillEllipse(Brushes.LightGray, 0, 0, 50, 50);
                    g.DrawEllipse(Pens.DarkGray, 0, 0, 49, 49);
                    
                    // Draw simple user icon
                    g.FillEllipse(Brushes.DarkGray, 15, 12, 20, 20);
                    g.FillEllipse(Brushes.DarkGray, 10, 30, 30, 15);
                }
                picUserImage.Image = bitmap;
            }
            catch
            {
                // If image creation fails, just leave it empty
            }
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnLogin.Enabled = false;
                btnLogin.Text = "Logging in...";

                string passwordHash = HashPassword(txtPassword.Text);
                var user = await _userRepository.GetUserByCredentialsAsync(txtUsername.Text, passwordHash);

                if (user != null)
                {
                    this.Hide();
                    var mainForm = new MainForm(user);
                    mainForm.FormClosed += (s, args) => this.Close();
                    mainForm.Show();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Login";
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
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