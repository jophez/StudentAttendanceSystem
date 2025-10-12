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
        private readonly IAttendanceRepository _attendanceRepository;
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
        private Label lblDebugInfo; // Add debug information label
        private System.Windows.Forms.Timer clockTimer;
        private Student? _currentStudent;
        private Image? _defaultStudentImage;
        private bool _isInitialized = false;
        private TextBox txtRFIDTag;
        private AttendanceValidationResult _lastValidationResult = new AttendanceValidationResult { IsValid = true, ValidationMessage = "" };
        private StudentAttendanceStatus? _lastAttendanceStatus = null;
        public AttendanceDisplayForm(DatabaseConnection dbConnection)
        {
            _studentRepository = new StudentRepository(dbConnection);
            _attendanceRepository = new AttendanceRepository(dbConnection);
            _smsRepository = new SMSRepository(dbConnection);

            // Initialize RFID service with delegates
            _rfidService = new RFIDService(
                async (rfidCode) =>
                {
                    try
                    {
                        var student = await _studentRepository.GetStudentByRFIDAsync(rfidCode);
                        UpdateDebugInfo($"Student lookup: {(student != null ? $"Found {student.FirstName} {student.LastName}" : "Not found")} for RFID: {rfidCode}");
                        return student;
                    }
                    catch (Exception ex)
                    {
                        UpdateDebugInfo($"Error looking up student: {ex.Message}");
                        return null;
                    }
                },
                async (studentId, type) =>
                {
                    try
                    {
                        _lastAttendanceStatus = await _attendanceRepository.GetStudentAttendanceStatusAsync(studentId);
                        _lastValidationResult = await _attendanceRepository.ValidateAttendanceActionAsync(studentId, (AttendanceType)Enum.Parse(typeof(AttendanceType), _lastAttendanceStatus.CurrentStatus));
                        if (!_lastValidationResult.IsValid)
                        {
                            UpdateDebugInfo($"Attendance validation failed: {_lastValidationResult.ValidationMessage}");
                            return false;
                        }
                        var result = await _attendanceRepository.RecordAttendanceAsync(studentId, (AttendanceType)Enum.Parse(typeof(AttendanceType), _lastAttendanceStatus.CurrentStatus));
                        UpdateDebugInfo($"Attendance recorded: Student {studentId}, Type: {type}");
                        return result;
                    }
                    catch (Exception ex)
                    {
                        UpdateDebugInfo($"Error recording attendance: {ex.Message}");
                        throw;
                    }
                },
                _attendanceRepository
            );

            InitializeComponent();
            SetupTimers();

            // Initialize services after form is shown
            this.Shown += async (s, e) => await InitializeServicesAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Student Attendance Tracking";
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
                Text = "Initializing RFID Scanner...",
                Font = new Font("Arial", 16, FontStyle.Italic),
                ForeColor = Color.Orange,
                Location = new Point(50, 80),
                Size = new Size(600, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Debug Info Label
            lblDebugInfo = new Label
            {
                Text = "Debug: System starting...",
                Font = new Font("Arial", 10),
                ForeColor = Color.Gray,
                Location = new Point(50, 110),
                Size = new Size(800, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Student Image
            picStudentImage = new PictureBox
            {
                Location = new Point(50, 140),
                Size = new Size(150, 150),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle
            };
            LoadDefaultStudentImage();

            // Student ID
            lblStudentId = new Label
            {
                Location = new Point(220, 140),
                Size = new Size(400, 30),
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "" // Start empty
            };

            // Student Name
            lblStudentName = new Label
            {
                Location = new Point(220, 180),
                Size = new Size(400, 40),
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Text = "" // Start empty
            };

            // Cell Phone
            lblCellPhone = new Label
            {
                Location = new Point(220, 230),
                Size = new Size(400, 25),
                Font = new Font("Arial", 12),
                ForeColor = Color.White,
                Text = "" // Start empty
            };

            // Email
            lblEmail = new Label
            {
                Location = new Point(220, 260),
                Size = new Size(400, 25),
                Font = new Font("Arial", 12),
                ForeColor = Color.White,
                Text = "" // Start empty
            };

            // Time In/Out Status
            lblTimeInOut = new Label
            {
                Location = new Point(50, 310),
                Size = new Size(600, 80),
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.Lime,
                Text = "" // Start empty
            };

            txtRFIDTag = new TextBox
            {
                Location = new Point(0, 0),
                Size = new Size(100, 100),
                Font = new Font("Arial", 12, FontStyle.Regular),
                Visible = true, // Hidden by default
                Text = string.Empty,
                ReadOnly = false
            };
            txtRFIDTag.KeyPress += TxtRFIDTag_KeyPress;

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                txtRFIDTag, lblCurrentTime, lblRFIDInstruction, lblDebugInfo, picStudentImage,
                lblStudentId, lblStudentName, lblCellPhone, lblEmail, lblTimeInOut
            });

            // Clear student display initially
            ClearStudentDisplay();

            this.ResumeLayout(false);
        }

        private async void TxtRFIDTag_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !string.IsNullOrWhiteSpace(txtRFIDTag.Text))
            {
                ClearStudentDisplay();
                // Simulate a card read when Enter is pressed
                if (txtRFIDTag.Text.Trim().Length > 0)
                {
                    var studentInfo = _studentRepository.GetStudentByRFIDAsync(txtRFIDTag.Text.Trim());
                    var args = new StudentAttendanceEventArgs
                    {
                        Student = await studentInfo,
                        ScanTime = DateTime.Parse(lblCurrentTime.Text),
                        Success = true
                    };
                    RfidService_StudentScanned(null, args);
                    e.Handled = true; // Prevent the beep sound on Enter key
                }
            }
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                UpdateDebugInfo("Initializing services...");

                var smsConfig = await _smsRepository.GetActiveSMSConfigurationAsync();
                UpdateDebugInfo($"SMS Config loaded: {(smsConfig != null ? "Success" : "None found")}");

                // Create SMS logger delegate
                Func<int?, string, string, SMSStatus, string?, Task<int>> smsLogger =
                    async (studentId, phoneNumber, message, status, errorMessage) =>
                    {
                        try
                        {
                            return await _smsRepository.LogSMSAsync(studentId, phoneNumber, message, status, errorMessage, null);
                        }
                        catch (Exception ex)
                        {
                            UpdateDebugInfo($"SMS Log Error: {ex.Message}");
                            return -1;
                        }
                    };

                var smsService = new SemaphoreSMSService(
                    smsConfig ?? new SMSConfiguration(),
                    smsLogger
                );

                _notificationService = new NotificationService(smsService, async () => await _smsRepository.GetActiveSMSConfigurationAsync());
                _notificationService.NotificationSent += NotificationService_NotificationSent;
                _notificationService.NotificationError += NotificationService_NotificationError;

                UpdateDebugInfo("Notification service initialized");

                await SetupRFIDServiceAsync();

                _isInitialized = true;
                UpdateDebugInfo("All services initialized successfully");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Initialization Error: {ex.Message}";
                lblRFIDInstruction.Text = errorMsg;
                lblRFIDInstruction.ForeColor = Color.Red;
                UpdateDebugInfo($"INIT ERROR: {ex}");
                System.Diagnostics.Debug.WriteLine($"Service Initialization Error: {ex}");
            }
        }

        private async Task SetupRFIDServiceAsync()
        {
            try
            {
                UpdateDebugInfo("Setting up RFID service...");

                // Subscribe to events BEFORE initializing
                _rfidService.StudentScanned += RfidService_StudentScanned;
                _rfidService.RFIDError += RfidService_RFIDError;
                _rfidService.StatusChanged += RfidService_StatusChanged;

                UpdateDebugInfo("RFID events subscribed, initializing...");

                // Initialize and start RFID reading
                var initialized = await _rfidService.InitializeAsync();
                UpdateDebugInfo($"RFID Initialize result: {initialized}");
                if (initialized)
                {
                    await _rfidService.StartReadingAsync();
                    lblRFIDInstruction.Text = "RFID Reader Ready - Please tap your card";
                    lblRFIDInstruction.ForeColor = Color.LightGreen;
                    UpdateDebugInfo("RFID service started successfully");
                }
                else
                {
                    lblRFIDInstruction.Text = "RFID Reader Not Available - Contact Administrator";
                    lblRFIDInstruction.ForeColor = Color.Red;
                    UpdateDebugInfo("RFID service failed to initialize");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"RFID Error: {ex.Message}";
                lblRFIDInstruction.Text = errorMsg;
                lblRFIDInstruction.ForeColor = Color.Red;
                UpdateDebugInfo($"RFID SETUP ERROR: {ex}");
                System.Diagnostics.Debug.WriteLine($"RFID Service Error: {ex}");
            }
        }

        private void UpdateDebugInfo(string message)
        {
            if (lblDebugInfo != null)
            {
                if (InvokeRequired)
                {
                    Invoke(() =>
                    {
                        lblDebugInfo.Text = $"Debug: {DateTime.Now:HH:mm:ss} - {message}";
                        System.Diagnostics.Debug.WriteLine($"[AttendanceDisplay] {message}");
                    });
                }
                else
                {
                    lblDebugInfo.Text = $"Debug: {DateTime.Now:HH:mm:ss} - {message}";
                    System.Diagnostics.Debug.WriteLine($"[AttendanceDisplay] {message}");
                }
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
            if (lblCurrentTime != null)
            {
                lblCurrentTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        private void RfidService_StudentScanned(object? sender, StudentAttendanceEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => RfidService_StudentScanned(sender, e));
                return;
            }

            UpdateDebugInfo($"Student scan event - Success: {e.Success}");

            if (e.Success && e.Student != null)
            {
                _currentStudent = e.Student;
                UpdateDebugInfo($"Displaying student: {e.Student.FirstName} {e.Student.LastName}");
                DisplayStudentInfo(e.Student);

                // Send SMS notification
                _ = Task.Run(async () =>
                {
                    try
                    {                       
                        _lastAttendanceStatus = await _attendanceRepository.GetStudentAttendanceStatusAsync(e.Student.StudentId);
                        _lastValidationResult = await _attendanceRepository.ValidateAttendanceActionAsync(e.Student.StudentId, (AttendanceType)Enum.Parse(typeof(AttendanceType), _lastAttendanceStatus.CurrentStatus));
                        if (_lastValidationResult.IsValid)
                        {
                            await _notificationService.SendAttendanceNotificationAsync(e.Student, (AttendanceType)Enum.Parse(typeof(AttendanceType), _lastAttendanceStatus.CurrentStatus), e.ScanTime);
                            var result = await _attendanceRepository.RecordAttendanceAsync(e.Student.StudentId, (AttendanceType)Enum.Parse(typeof(AttendanceType), _lastAttendanceStatus.CurrentStatus));
                            UpdateDebugInfo("SMS notification sent");
                        }
                        else
                        {
                            UpdateDebugInfo($"Attendance validation failed: {_lastValidationResult.ValidationMessage}");
                            UpdateDebugInfo($"Attendance recorded: Student {e.Student.StudentId}, Type: {e.AttendanceType}");
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateDebugInfo($"SMS notification failed: {ex.Message}");
                    }
                });

                var typeText = _lastAttendanceStatus == null ? "TIME_IN" : string.Join("_", new string[] { "TIME", _lastAttendanceStatus.CurrentStatus.ToUpper() });
                lblTimeInOut.Text = $"✓ {typeText} RECORDED\nTime: {e.ScanTime:HH:mm:ss}\nDate: {e.ScanTime:yyyy-MM-dd}";
                lblTimeInOut.ForeColor = Color.Lime;
            }
            else
            {
                var errorMsg = e.ErrorMessage ?? "Unknown error";
                lblTimeInOut.Text = $"✗ ATTENDANCE FAILED\n{errorMsg}";
                lblTimeInOut.ForeColor = Color.Red;
                UpdateDebugInfo($"Attendance failed: {errorMsg}");

                //_ = Task.Run(async () =>
                //{
                //    await Task.Delay(3000);
                //    if (InvokeRequired)
                //        Invoke(ClearStudentDisplay);
                //    else
                //        ClearStudentDisplay();
                //});
            }
        }

        private void RfidService_RFIDError(object? sender, Core.Interfaces.RFIDErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => RfidService_RFIDError(sender, e));
                return;
            }

            var errorMsg = e.ErrorMessage ?? "Unknown RFID error";
            lblTimeInOut.Text = $"✗ RFID ERROR\n{errorMsg}";
            lblTimeInOut.ForeColor = Color.Red;
            UpdateDebugInfo($"RFID Error: {errorMsg}");

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

            var statusMsg = e.Message ?? "";
            UpdateDebugInfo($"RFID Status: {e.Status} - {statusMsg}");

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
                    lblRFIDInstruction.Text = $"RFID Reader Error: {statusMsg}";
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
            try
            {
                if (student == null)
                {
                    UpdateDebugInfo("DisplayStudentInfo called with null student");
                    return;
                }

                UpdateDebugInfo($"Displaying info for: {student.FirstName} {student.LastName}");

                // Ensure we're on the UI thread
                if (InvokeRequired)
                {
                    Invoke(() => DisplayStudentInfo(student));
                    return;
                }

                lblStudentId.Text = $"Student ID: {student.StudentNumber ?? "N/A"}";

                var fullName = $"{student.FirstName ?? ""} {student.MiddleName ?? ""} {student.LastName ?? ""}".Trim();
                lblStudentName.Text = string.IsNullOrWhiteSpace(fullName) ? "No Name Available" : fullName;

                lblCellPhone.Text = $"Phone: {student.CellPhone ?? "N/A"}";
                lblEmail.Text = $"Email: {student.Email ?? "N/A"}";

                // Load student image
                LoadStudentImage(student.ImagePath);

                UpdateDebugInfo("Student info displayed successfully");
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error displaying student info: {ex.Message}");
            }
        }

        private void LoadStudentImage(string? imagePath)
        {
            try
            {
                // Dispose previous image to prevent memory leaks
                if (picStudentImage.Image != null && picStudentImage.Image != _defaultStudentImage)
                {
                    var oldImage = picStudentImage.Image;
                    picStudentImage.Image = null;
                    oldImage.Dispose();
                }

                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    UpdateDebugInfo($"Loading image from: {imagePath}");
                    picStudentImage.Image = Image.FromFile(imagePath);
                }
                else
                {
                    UpdateDebugInfo($"Using default image (path: {imagePath ?? "null"})");
                    picStudentImage.Image = GetDefaultStudentImage();
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error loading image: {ex.Message}");
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

                        using (var font = new Font("Arial", 10))
                        {
                            g.DrawString("No Photo", font, Brushes.White, 45, 135);
                        }
                    }
                    _defaultStudentImage = bitmap;
                }
                catch (Exception ex)
                {
                    UpdateDebugInfo($"Error creating default image: {ex.Message}");
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
            if (InvokeRequired)
            {
                Invoke(ClearStudentDisplay);
                return;
            }

            lblStudentId.Text = "";
            lblStudentName.Text = "";
            lblCellPhone.Text = "";
            lblEmail.Text = "";
            lblTimeInOut.Text = "";
            txtRFIDTag.SelectAll();
            LoadDefaultStudentImage();
        }

        private void NotificationService_NotificationSent(object? sender, NotificationEventArgs e)
        {
            UpdateDebugInfo($"SMS sent to {e.PhoneNumber} for {e.Student.FirstName} {e.Student.LastName}");
        }

        private void NotificationService_NotificationError(object? sender, NotificationErrorEventArgs e)
        {
            UpdateDebugInfo($"SMS Error: {e.ErrorMessage}");
        }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                UpdateDebugInfo("Form closing...");

                _cancellationTokenSource?.Cancel();

                clockTimer?.Stop();
                clockTimer?.Dispose();

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
                _cancellationTokenSource?.Dispose();
                _defaultStudentImage?.Dispose();

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

            //// Add test key for debugging
            //if (keyData == Keys.F12)
            //{
            //    // Simulate a student scan for testing
            //    _ = Task.Run(async () => await TestStudentScan());
            //    return true;
            //}

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}