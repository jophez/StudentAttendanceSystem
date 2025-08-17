using StudentAttendanceSystem.Core.Models;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class StudentDetailForm : Form
    {
        private readonly Student _student;
        private PictureBox picStudentImage;
        private Label lblStudentId;
        private Label lblStudentNumber;
        private Label lblFirstName;
        private Label lblMiddleName;
        private Label lblLastName;
        private Label lblCellPhone;
        private Label lblEmail;
        private Label lblAddress;
        private Label lblGuardianName;
        private Label lblGuardianCellPhone;
        private Label lblGuardianEmail;
        private Label lblTimeInOut;
        private Button btnClose;

        public StudentDetailForm(Student student)
        {
            _student = student;
            InitializeComponent();
            LoadStudentData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Student Details";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 600);
            this.BackColor = Color.White;

            // Student Image
            picStudentImage = new PictureBox
            {
                Location = new Point(200, 20),
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle
            };
            LoadStudentImage();

            // Student ID Label
            lblStudentId = new Label
            {
                Location = new Point(30, 140),
                Size = new Size(400, 25),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            // Student Number
            lblStudentNumber = new Label
            {
                Location = new Point(30, 170),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10)
            };

            // First Name
            lblFirstName = new Label
            {
                Location = new Point(30, 195),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10)
            };

            // Middle Name
            lblMiddleName = new Label
            {
                Location = new Point(30, 220),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10)
            };

            // Last Name
            lblLastName = new Label
            {
                Location = new Point(30, 245),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10)
            };

            // Cell Phone
            lblCellPhone = new Label
            {
                Location = new Point(30, 270),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10)
            };

            // Email
            lblEmail = new Label
            {
                Location = new Point(30, 295),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10)
            };

            // Address
            lblAddress = new Label
            {
                Location = new Point(30, 320),
                Size = new Size(400, 40),
                Font = new Font("Arial", 10)
            };

            // Guardian Name
            lblGuardianName = new Label
            {
                Location = new Point(30, 380),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };

            // Guardian Cell Phone
            lblGuardianCellPhone = new Label
            {
                Location = new Point(30, 405),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10)
            };

            // Guardian Email
            lblGuardianEmail = new Label
            {
                Location = new Point(30, 430),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10)
            };

            // Time In/Out
            lblTimeInOut = new Label
            {
                Location = new Point(30, 470),
                Size = new Size(400, 40),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Red
            };

            // Close Button
            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(200, 520),
                Size = new Size(100, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += BtnClose_Click;

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                picStudentImage, lblStudentId, lblStudentNumber, lblFirstName, lblMiddleName,
                lblLastName, lblCellPhone, lblEmail, lblAddress, lblGuardianName,
                lblGuardianCellPhone, lblGuardianEmail, lblTimeInOut, btnClose
            });

            this.ResumeLayout(false);
        }

        private void LoadStudentImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(_student.ImagePath) && File.Exists(_student.ImagePath))
                {
                    picStudentImage.Image = Image.FromFile(_student.ImagePath);
                }
                else
                {
                    // Create a default student image
                    var bitmap = new Bitmap(100, 100);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.FillRectangle(Brushes.LightGray, 0, 0, 100, 100);
                        g.DrawRectangle(Pens.DarkGray, 0, 0, 99, 99);
                        
                        // Draw simple student icon
                        g.FillEllipse(Brushes.DarkGray, 35, 20, 30, 30);
                        g.FillRectangle(Brushes.DarkGray, 30, 60, 40, 30);
                        
                        var font = new Font("Arial", 8);
                        g.DrawString("No Photo", font, Brushes.Black, 25, 85);
                    }
                    picStudentImage.Image = bitmap;
                }
            }
            catch
            {
                // If image loading fails, create a simple placeholder
                var bitmap = new Bitmap(100, 100);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.FillRectangle(Brushes.LightGray, 0, 0, 100, 100);
                    g.DrawString("Image Error", new Font("Arial", 8), Brushes.Red, 10, 45);
                }
                picStudentImage.Image = bitmap;
            }
        }

        private void LoadStudentData()
        {
            lblStudentId.Text = $"Student ID: {_student.StudentId}";
            lblStudentNumber.Text = $"Student Number: {_student.StudentNumber}";
            lblFirstName.Text = $"First Name: {_student.FirstName}";
            lblMiddleName.Text = $"Middle Name: {_student.MiddleName}";
            lblLastName.Text = $"Last Name: {_student.LastName}";
            lblCellPhone.Text = $"Cell Phone: {_student.CellPhone}";
            lblEmail.Text = $"Email: {_student.Email}";
            
            lblAddress.Text = $"Address: {_student.StreetAddress}, {_student.Barangay}, " +
                            $"{_student.Municipality}, {_student.City}";

            if (_student.Guardian != null)
            {
                lblGuardianName.Text = $"Guardian: {_student.Guardian.FirstName} {_student.Guardian.LastName}";
                lblGuardianCellPhone.Text = $"Guardian Phone: {_student.Guardian.CellPhone}";
                lblGuardianEmail.Text = $"Guardian Email: {_student.Guardian.Email}";
            }
            else
            {
                lblGuardianName.Text = "Guardian: Not Assigned";
                lblGuardianCellPhone.Text = "Guardian Phone: N/A";
                lblGuardianEmail.Text = "Guardian Email: N/A";
            }

            // This would normally load from attendance records
            lblTimeInOut.Text = "Time In/Out: Not Available Today\n(RFID scanning functionality to be implemented)";
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}