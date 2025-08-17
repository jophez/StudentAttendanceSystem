using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Core.Services;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;
using System.Runtime.CompilerServices;
using System.Threading;
using SMSStatus = StudentAttendanceSystem.Core.Models.SMSStatus;

namespace StudentAttendanceSystem.Service.Forms
{
    public partial class AttendanceDisplayForm : Form
    {
        private readonly StudentRepository _studentRepository;
        private readonly AttendanceRepository _attendanceRepository;
        private readonly SMSRepository _smsRepository;
        private readonly RFIDService _rfidService;
        private NotificationService _notificationService;
        private Label lblCurrentTime;
        private PictureBox picStudentImage;
        private Label lblStudentId;
        private Label lblStudentName;
        private Label lblCellPhone;
        private Label lblEmail;
        private Label lblTimeInOut;
        private Label lblRFIDInstruction;
        private System.Windows.Forms.Timer clockTimer;
        private Student? _currentStudent;
        private Image? _defaultStudentImage;

        public AttendanceDisplayForm(DatabaseConnection dbConnection)
        {
            _studentRepository = new StudentRepository(dbConnection);
            _attendanceRepository = new AttendanceRepository(dbConnection);
            _smsRepository = new SMSRepository(dbConnection);

            // Initialize RFID service with delegates
            _rfidService = new RFIDService(
                async (rfidCode) => await _studentRepository.GetStudentByRFIDAsync(rfidCode),
                async (studentId, type) => await _attendanceRepository.RecordAttendanceAsync(studentId, type)
            );

            // FIXED: Don't call async methods in constructor
            InitializeComponent();
            SetupTimers();

            // Initialize services after form is loaded
            this.Load += async (s, e) => await InitializeServicesAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Student Attendance Display";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Navy;
            this.ForeColor = Color.White;

            // Current Time Label (centered at top)
            lblCurrentTime = new Label
            {
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.Yellow,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // RFID Instruction
            lblRFIDInstruction = new Label
            {
                Text = "Please tap your RFID card to scan attendance",
                Font = new Font("Arial", 16, FontStyle.Italic),
                ForeColor = Color.LightGray,
                Location = new Point(50, 80),
                Size = new Size(600, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Student Image
            picStudentImage = new PictureBox
            {
                Location = new Point(50, 130),
                Size = new Size(150, 150),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle
            };
            LoadDefaultStudentImage();

            // Student ID
            lblStudentId = new Label
            {
                Location = new Point(220, 130),
                Size = new Size(400, 30),
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White
            };

            // Student Name
            lblStudentName = new Label
            {
                Location = new Point(220, 170),
                Size = new Size(400, 40),
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.Cyan
            };

            // Cell Phone
            lblCellPhone = new Label
            {
                Location = new Point(220, 220),
                Size = new Size(400, 25),
                Font = new Font("Arial", 12),
                ForeColor = Color.White
            };

            // Email
            lblEmail = new Label
            {
                Location = new Point(220, 250),
                Size = new Size(400, 25),
                Font = new Font("Arial", 12),
                ForeColor = Color.White
            };

            // Time In/Out Status
            lblTimeInOut = new Label
            {
                Location = new Point(50, 300),
                Size = new Size(600, 60),
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.Lime
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblCurrentTime, lblRFIDInstruction, picStudentImage, lblStudentId,
                lblStudentName, lblCellPhone, lblEmail, lblTimeInOut
            });

            // Clear student display initially
            ClearStudentDisplay();

            this.ResumeLayout(false);
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                var smsConfig = await _smsRepository.GetActiveSMSConfigurationAsync();

                // The constructor expects: Func<int?, string, string, SMSStatus, string?, Task<int>>
                // This matches your LogSMSAsync signature, but it seems to only take 5 parameters, not 6
                // Based on the tooltip, it looks like it expects: (studentId, phoneNumber, message, status, errorMessage)

                Func<int?, string, string, SMSStatus, string?, Task<int>> smsLogger =
                    async (studentId, phoneNumber, message, status, errorMessage) =>
                    {
                        try
                        {
                            // Call your repository method with the 5 parameters, defaulting providerResponse to null
                            return await _smsRepository.LogSMSAsync(studentId, phoneNumber, message, status, errorMessage, null);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error logging SMS: {ex.Message}");
                            return -1; // Return error indicator
                        }
                    };

                var smsService = new SemaphoreSMSService(
                    smsConfig ?? new SMSConfiguration(),
                    smsLogger  // This should now match the expected signature exactly
                );

                _notificationService = new NotificationService(smsService, async () => await _smsRepository.GetActiveSMSConfigurationAsync());
                _notificationService.NotificationSent += NotificationService_NotificationSent;
                _notificationService.NotificationError += NotificationService_NotificationError;

                await SetupRFIDServiceAsync();

                System.Diagnostics.Debug.WriteLine("Services initialized successfully");
            }
            catch (Exception ex)
            {
                lblRFIDInstruction.Text = $"Initialization Error: {ex.Message}";
                lblRFIDInstruction.ForeColor = Color.Red;
                System.Diagnostics.Debug.WriteLine($"Service Initialization Error: {ex}");
            }
        }

        private async Task SetupRFIDServiceAsync()
        {
            try
            {
                _rfidService.StudentScanned += RfidService_StudentScanned;
                _rfidService.RFIDError += RfidService_RFIDError;
                _rfidService.StatusChanged += RfidService_StatusChanged;

                // Initialize and start RFID reading
                var initialized = await _rfidService.InitializeAsync();
                if (initialized)
                {
                    await _rfidService.StartReadingAsync();
                    lblRFIDInstruction.Text = "RFID Reader Ready - Please tap your card";
                    lblRFIDInstruction.ForeColor = Color.LightGreen;
                }
                else
                {
                    lblRFIDInstruction.Text = "RFID Reader Not Available - Contact Administrator";
                    lblRFIDInstruction.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblRFIDInstruction.Text = $"RFID Error: {ex.Message}";
                lblRFIDInstruction.ForeColor = Color.Red;
                System.Diagnostics.Debug.WriteLine($"RFID Service Error: {ex}");
            }
        }

        private void SetupTimers()
        {
            // Clock timer - updates every second
            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            lblCurrentTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void RfidService_StudentScanned(object? sender, StudentAttendanceEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => RfidService_StudentScanned(sender, e));
                return;
            }

            if (e.Success)
            {
                _currentStudent = e.Student;
                DisplayStudentInfo(e.Student);
                
                var typeText = e.AttendanceType == AttendanceType.TimeIn ? "TIME IN" : "TIME OUT";
                lblTimeInOut.Text = $"✓ {typeText} RECORDED\nTime: {e.ScanTime:HH:mm:ss}\nDate: {e.ScanTime:yyyy-MM-dd}";
                lblTimeInOut.ForeColor = Color.Lime;

                // Send SMS notification
                _ = Task.Run(async () => await _notificationService.SendAttendanceNotificationAsync(e.Student, e.AttendanceType, e.ScanTime));

                // Clear display after 5 seconds
                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    if (InvokeRequired)
                        Invoke(ClearStudentDisplay);
                    else
                        ClearStudentDisplay();
                });
            }
            else
            {
                lblTimeInOut.Text = $"❌ ATTENDANCE FAILED\n{e.ErrorMessage}";
                lblTimeInOut.ForeColor = Color.Red;
                
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    if (InvokeRequired)
                        Invoke(ClearStudentDisplay);
                    else
                        ClearStudentDisplay();
                });
            }
        }

        private void RfidService_RFIDError(object? sender, Core.Interfaces.RFIDErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => RfidService_RFIDError(sender, e));
                return;
            }

            lblTimeInOut.Text = $"❌ RFID ERROR\n{e.ErrorMessage}";
            lblTimeInOut.ForeColor = Color.Red;
            
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                if (InvokeRequired)
                    Invoke(ClearStudentDisplay);
                else
                    ClearStudentDisplay();
            });
        }

        private void RfidService_StatusChanged(object? sender, Core.Interfaces.RFIDStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => RfidService_StatusChanged(sender, e));
                return;
            }

            switch (e.Status)
            {
                case Core.Interfaces.RFIDReaderStatus.Reading:
                    lblRFIDInstruction.Text = "RFID Reader Active - Please tap your card";
                    lblRFIDInstruction.ForeColor = Color.LightGreen;
                    break;
                case Core.Interfaces.RFIDReaderStatus.Connected:
                    lblRFIDInstruction.Text = "RFID Reader Connected - Ready to scan";
                    lblRFIDInstruction.ForeColor = Color.Yellow;
                    break;
                case Core.Interfaces.RFIDReaderStatus.Error:
                    lblRFIDInstruction.Text = $"RFID Reader Error: {e.Message}";
                    lblRFIDInstruction.ForeColor = Color.Red;
                    break;
                case Core.Interfaces.RFIDReaderStatus.Disconnected:
                    lblRFIDInstruction.Text = "RFID Reader Disconnected - Contact Administrator";
                    lblRFIDInstruction.ForeColor = Color.Red;
                    break;
            }
        }


        private void DisplayStudentInfo(Student student)
        {
            lblStudentId.Text = $"Student ID: {student.StudentNumber}";
            lblStudentName.Text = $"{student.FirstName} {student.MiddleName} {student.LastName}";
            lblCellPhone.Text = $"Phone: {student.CellPhone}";
            lblEmail.Text = $"Email: {student.Email}";

            // Load student image
            LoadStudentImage(student.ImagePath);
        }

        private void LoadStudentImage(string? imagePath)
        {
            try
            {
                // FIXED: Properly dispose previous image to prevent memory leaks
                if (picStudentImage.Image != null && picStudentImage.Image != _defaultStudentImage)
                {
                    var oldImage = picStudentImage.Image;
                    picStudentImage.Image = null;
                    oldImage.Dispose();
                }

                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    picStudentImage.Image = Image.FromFile(imagePath);
                }
                else
                {
                    picStudentImage.Image = GetDefaultStudentImage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading student image: {ex.Message}");
                picStudentImage.Image = GetDefaultStudentImage();
            }
        }

        private Image GetDefaultStudentImage()
        {
            if (_defaultStudentImage == null)
            {
                try
                {
                    var bitmap = new Bitmap(150, 150);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.FillRectangle(Brushes.DarkGray, 0, 0, 150, 150);
                        g.DrawRectangle(Pens.White, 0, 0, 149, 149);

                        // Draw simple student icon
                        g.FillEllipse(Brushes.LightGray, 50, 30, 50, 50);
                        g.FillRectangle(Brushes.LightGray, 40, 90, 70, 40);

                        // FIXED: Properly dispose font
                        using (var font = new Font("Arial", 10))
                        {
                            g.DrawString("No Photo", font, Brushes.White, 45, 135);
                        }
                    }
                    _defaultStudentImage = bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating default image: {ex.Message}");
                    // Return a simple colored rectangle if image creation fails
                    var fallback = new Bitmap(150, 150);
                    using (var g = Graphics.FromImage(fallback))
                    {
                        g.Clear(Color.DarkGray);
                    }
                    _defaultStudentImage = fallback;
                }
            }
            return _defaultStudentImage;
        }

        private void LoadDefaultStudentImage()
        {
            picStudentImage.Image = GetDefaultStudentImage();
        }

        private void ClearStudentDisplay()
        {
            lblStudentId.Text = "";
            lblStudentName.Text = "";
            lblCellPhone.Text = "";
            lblEmail.Text = "";
            lblTimeInOut.Text = "";
            LoadDefaultStudentImage();
        }

        private void NotificationService_NotificationSent(object? sender, NotificationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"✓ SMS sent to {e.PhoneNumber} for student {e.Student.FirstName} {e.Student.LastName}");
        }

        private void NotificationService_NotificationError(object? sender, NotificationErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"✗ SMS Error: {e.ErrorMessage}");
        }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                // Cancel any ongoing operations
                _cancellationTokenSource?.Cancel();

                clockTimer?.Stop();
                clockTimer?.Dispose();

                // Proper async disposal handling
                if (_rfidService != null)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _rfidService.StopReadingAsync();
                            _rfidService.Dispose();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error disposing RFID service: {ex}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in form cleanup: {ex}");
            }

            base.OnFormClosed(e);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of cached default image
                _cancellationTokenSource?.Dispose();
                _defaultStudentImage?.Dispose();

                // Dispose of current student image if it's not the default
                if (picStudentImage?.Image != null && picStudentImage.Image != _defaultStudentImage)
                {
                    picStudentImage.Image.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        // Handle Escape key to close form (for testing purposes)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}