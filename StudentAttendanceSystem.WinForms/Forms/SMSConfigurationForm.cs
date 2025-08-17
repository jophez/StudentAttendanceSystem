using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Core.Services;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Data.Repositories;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class SMSConfigurationForm : Form
    {
        private readonly SMSRepository _smsRepository;
        private SMSConfiguration? _currentConfig;
        private ISMSService? _smsService;

        // UI Controls
        private GroupBox gbProviderSettings;
        private Label lblProviderName;
        private TextBox txtProviderName;
        private Label lblApiKey;
        private TextBox txtApiKey;
        private Label lblApiUrl;
        private TextBox txtApiUrl;
        private Label lblSenderName;
        private TextBox txtSenderName;
        private CheckBox chkIsActive;

        private GroupBox gbTestSection;
        private Label lblTestPhone;
        private TextBox txtTestPhone;
        private Label lblTestMessage;
        private TextBox txtTestMessage;
        private Button btnTestConnection;
        private Button btnSendTestSMS;
        private Button btnCheckBalance;

        private GroupBox gbStatistics;
        private Label lblBalance;
        private Label lblSentToday;
        private Label lblLastTest;
        private Label lblConnectionStatus;

        private GroupBox gbMessageTemplates;
        private Label lblTimeInTemplate;
        private TextBox txtTimeInTemplate;
        private Label lblTimeOutTemplate;
        private TextBox txtTimeOutTemplate;
        private Button btnPreviewTemplate;

        private Button btnSave;
        private Button btnCancel;
        private Button btnViewLogs;

        public SMSConfigurationForm()
        {
            var dbConnection = new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString());
            _smsRepository = new SMSRepository(dbConnection);
            InitializeComponent();
            LoadConfiguration();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "SMS Provider Configuration - Semaphore";
            this.Size = new Size(700, 650);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Provider Settings Group
            gbProviderSettings = new GroupBox
            {
                Text = "Semaphore API Settings",
                Location = new Point(20, 20),
                Size = new Size(640, 160),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblProviderName = new Label
            {
                Text = "Provider Name:",
                Location = new Point(15, 30),
                Size = new Size(100, 20),
                Font = new Font("Arial", 9)
            };

            txtProviderName = new TextBox
            {
                Text = "Semaphore",
                Location = new Point(125, 28),
                Size = new Size(200, 25),
                Font = new Font("Arial", 9),
                ReadOnly = true,
                BackColor = Color.LightGray
            };

            lblApiKey = new Label
            {
                Text = "API Key:",
                Location = new Point(15, 65),
                Size = new Size(100, 20),
                Font = new Font("Arial", 9)
            };

            txtApiKey = new TextBox
            {
                Location = new Point(125, 63),
                Size = new Size(300, 25),
                Font = new Font("Arial", 9),
                UseSystemPasswordChar = true
            };

            lblApiUrl = new Label
            {
                Text = "API URL:",
                Location = new Point(15, 100),
                Size = new Size(100, 20),
                Font = new Font("Arial", 9)
            };

            txtApiUrl = new TextBox
            {
                Text = "https://api.semaphore.co/api/v4/messages",
                Location = new Point(125, 98),
                Size = new Size(300, 25),
                Font = new Font("Arial", 9)
            };

            lblSenderName = new Label
            {
                Text = "Sender Name:",
                Location = new Point(450, 30),
                Size = new Size(80, 20),
                Font = new Font("Arial", 9)
            };

            txtSenderName = new TextBox
            {
                Location = new Point(540, 28),
                Size = new Size(80, 25),
                Font = new Font("Arial", 9),
                MaxLength = 11 // Semaphore sender name limit
            };

            chkIsActive = new CheckBox
            {
                Text = "Active Configuration",
                Location = new Point(450, 65),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9),
                Checked = true
            };

            gbProviderSettings.Controls.AddRange(new Control[] {
                lblProviderName, txtProviderName, lblApiKey, txtApiKey,
                lblApiUrl, txtApiUrl, lblSenderName, txtSenderName, chkIsActive
            });

            // Test Section Group
            gbTestSection = new GroupBox
            {
                Text = "Testing & Validation",
                Location = new Point(20, 190),
                Size = new Size(640, 120),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblTestPhone = new Label
            {
                Text = "Test Phone:",
                Location = new Point(15, 30),
                Size = new Size(80, 20),
                Font = new Font("Arial", 9)
            };

            txtTestPhone = new TextBox
            {
                Location = new Point(100, 28),
                Size = new Size(150, 25),
                Font = new Font("Arial", 9),
                PlaceholderText = "+639123456789"
            };

            lblTestMessage = new Label
            {
                Text = "Test Message:",
                Location = new Point(15, 65),
                Size = new Size(80, 20),
                Font = new Font("Arial", 9)
            };

            txtTestMessage = new TextBox
            {
                Text = "This is a test message from School Attendance System.",
                Location = new Point(100, 63),
                Size = new Size(350, 25),
                Font = new Font("Arial", 9)
            };

            btnTestConnection = new Button
            {
                Text = "Test Connection",
                Location = new Point(470, 28),
                Size = new Size(110, 25),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                Font = new Font("Arial", 8, FontStyle.Bold)
            };
            btnTestConnection.Click += BtnTestConnection_Click;

            btnSendTestSMS = new Button
            {
                Text = "Send Test SMS",
                Location = new Point(470, 63),
                Size = new Size(110, 25),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 8, FontStyle.Bold)
            };
            btnSendTestSMS.Click += BtnSendTestSMS_Click;

            btnCheckBalance = new Button
            {
                Text = "Check Balance",
                Location = new Point(470, 93),
                Size = new Size(110, 25),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Font = new Font("Arial", 8, FontStyle.Bold)
            };
            btnCheckBalance.Click += BtnCheckBalance_Click;

            gbTestSection.Controls.AddRange(new Control[] {
                lblTestPhone, txtTestPhone, lblTestMessage, txtTestMessage,
                btnTestConnection, btnSendTestSMS, btnCheckBalance
            });

            // Statistics Group
            gbStatistics = new GroupBox
            {
                Text = "Status & Statistics",
                Location = new Point(20, 320),
                Size = new Size(640, 80),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblConnectionStatus = new Label
            {
                Text = "Connection: Not Tested",
                Location = new Point(15, 25),
                Size = new Size(200, 20),
                Font = new Font("Arial", 9),
                ForeColor = Color.Gray
            };

            lblBalance = new Label
            {
                Text = "Balance: Unknown",
                Location = new Point(15, 50),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9)
            };

            lblSentToday = new Label
            {
                Text = "Sent Today: Loading...",
                Location = new Point(220, 25),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9)
            };

            lblLastTest = new Label
            {
                Text = "Last Test: None",
                Location = new Point(220, 50),
                Size = new Size(200, 20),
                Font = new Font("Arial", 9)
            };

            gbStatistics.Controls.AddRange(new Control[] {
                lblConnectionStatus, lblBalance, lblSentToday, lblLastTest
            });

            // Message Templates Group
            gbMessageTemplates = new GroupBox
            {
                Text = "Message Templates",
                Location = new Point(20, 410),
                Size = new Size(640, 120),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblTimeInTemplate = new Label
            {
                Text = "Time In Message:",
                Location = new Point(15, 30),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9)
            };

            txtTimeInTemplate = new TextBox
            {
                Text = "Your child {StudentName} has arrived at school at {Time} on {Date}. - School Attendance System",
                Location = new Point(140, 28),
                Size = new Size(420, 25),
                Font = new Font("Arial", 9)
            };

            lblTimeOutTemplate = new Label
            {
                Text = "Time Out Message:",
                Location = new Point(15, 65),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9)
            };

            txtTimeOutTemplate = new TextBox
            {
                Text = "Your child {StudentName} has left school at {Time} on {Date}. - School Attendance System",
                Location = new Point(140, 63),
                Size = new Size(420, 25),
                Font = new Font("Arial", 9)
            };

            btnPreviewTemplate = new Button
            {
                Text = "Preview",
                Location = new Point(570, 90),
                Size = new Size(60, 25),
                BackColor = Color.Purple,
                ForeColor = Color.White,
                Font = new Font("Arial", 8, FontStyle.Bold)
            };
            btnPreviewTemplate.Click += BtnPreviewTemplate_Click;

            gbMessageTemplates.Controls.AddRange(new Control[] {
                lblTimeInTemplate, txtTimeInTemplate, lblTimeOutTemplate, txtTimeOutTemplate, btnPreviewTemplate
            });

            // Action Buttons
            btnSave = new Button
            {
                Text = "Save Configuration",
                Location = new Point(380, 550),
                Size = new Size(130, 35),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(520, 550),
                Size = new Size(80, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnCancel.Click += BtnCancel_Click;

            btnViewLogs = new Button
            {
                Text = "View SMS Logs",
                Location = new Point(20, 550),
                Size = new Size(120, 35),
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnViewLogs.Click += BtnViewLogs_Click;

            // Add all controls to form
            this.Controls.AddRange(new Control[] {
                gbProviderSettings, gbTestSection, gbStatistics, gbMessageTemplates,
                btnSave, btnCancel, btnViewLogs
            });

            this.ResumeLayout(false);
        }

        private async void LoadConfiguration()
        {
            try
            {
                _currentConfig = await _smsRepository.GetActiveSMSConfigurationAsync();
                
                if (_currentConfig != null)
                {
                    txtProviderName.Text = _currentConfig.ProviderName;
                    txtApiKey.Text = _currentConfig.ApiKey;
                    txtApiUrl.Text = _currentConfig.ApiUrl;
                    txtSenderName.Text = _currentConfig.SenderName;
                    chkIsActive.Checked = _currentConfig.IsActive;
                }

                // Load statistics
                var sentToday = await _smsRepository.GetSMSCountTodayAsync();
                lblSentToday.Text = $"Sent Today: {sentToday}";

                // Initialize SMS service if configuration exists
                if (_currentConfig != null)
                {
                    InitializeSMSService();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeSMSService()
        {
            if (_currentConfig == null) return;

            _smsService = new SemaphoreSMSService(_currentConfig, _smsRepository.LogSMSAsync);
        }

        private async void BtnTestConnection_Click(object sender, EventArgs e)
        {
            if (_smsService == null)
            {
                MessageBox.Show("Please save the configuration first.", "Configuration Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnTestConnection.Enabled = false;
                btnTestConnection.Text = "Testing...";
                lblConnectionStatus.Text = "Connection: Testing...";
                lblConnectionStatus.ForeColor = Color.Orange;

                var isConnected = await _smsService.TestConnectionAsync();
                
                if (isConnected)
                {
                    lblConnectionStatus.Text = "Connection: Success";
                    lblConnectionStatus.ForeColor = Color.Green;
                    lblLastTest.Text = $"Last Test: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    MessageBox.Show("Connection test successful!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblConnectionStatus.Text = "Connection: Failed";
                    lblConnectionStatus.ForeColor = Color.Red;
                    MessageBox.Show("Connection test failed. Please check your API key and URL.", "Connection Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = "Connection: Error";
                lblConnectionStatus.ForeColor = Color.Red;
                MessageBox.Show($"Connection test error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestConnection.Enabled = true;
                btnTestConnection.Text = "Test Connection";
            }
        }

        private async void BtnSendTestSMS_Click(object sender, EventArgs e)
        {
            if (_smsService == null)
            {
                MessageBox.Show("Please save the configuration first.", "Configuration Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTestPhone.Text) || string.IsNullOrWhiteSpace(txtTestMessage.Text))
            {
                MessageBox.Show("Please enter both phone number and test message.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnSendTestSMS.Enabled = false;
                btnSendTestSMS.Text = "Sending...";

                var result = await _smsService.SendSMSAsync(txtTestPhone.Text, txtTestMessage.Text);
                
                if (result.Success)
                {
                    MessageBox.Show($"Test SMS sent successfully!\nMessage ID: {result.MessageId}", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Refresh sent count
                    var sentToday = await _smsRepository.GetSMSCountTodayAsync();
                    lblSentToday.Text = $"Sent Today: {sentToday}";
                }
                else
                {
                    MessageBox.Show($"Failed to send test SMS:\n{result.ErrorMessage}", "Send Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending test SMS: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSendTestSMS.Enabled = true;
                btnSendTestSMS.Text = "Send Test SMS";
            }
        }

        private async void BtnCheckBalance_Click(object sender, EventArgs e)
        {
            if (_smsService == null)
            {
                MessageBox.Show("Please save the configuration first.", "Configuration Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnCheckBalance.Enabled = false;
                btnCheckBalance.Text = "Checking...";

                var balance = await _smsService.GetBalanceAsync();
                lblBalance.Text = $"Balance: ₱{balance:F2}";
                
                MessageBox.Show($"Current balance: ₱{balance:F2}", "Balance Check",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking balance: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCheckBalance.Enabled = true;
                btnCheckBalance.Text = "Check Balance";
            }
        }

        private void BtnPreviewTemplate_Click(object sender, EventArgs e)
        {
            var sampleName = "Juan Dela Cruz";
            var sampleTime = DateTime.Now.ToString("HH:mm");
            var sampleDate = DateTime.Now.ToString("yyyy-MM-dd");

            var timeInPreview = txtTimeInTemplate.Text
                .Replace("{StudentName}", sampleName)
                .Replace("{Time}", sampleTime)
                .Replace("{Date}", sampleDate);

            var timeOutPreview = txtTimeOutTemplate.Text
                .Replace("{StudentName}", sampleName)
                .Replace("{Time}", sampleTime)
                .Replace("{Date}", sampleDate);

            var preview = $"Time In Message:\n{timeInPreview}\n\nTime Out Message:\n{timeOutPreview}";
            
            MessageBox.Show(preview, "Message Template Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtApiKey.Text) || string.IsNullOrWhiteSpace(txtApiUrl.Text) ||
                string.IsNullOrWhiteSpace(txtSenderName.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var config = new SMSConfiguration
                {
                    ConfigId = _currentConfig?.ConfigId ?? 0,
                    ProviderName = txtProviderName.Text,
                    ApiKey = txtApiKey.Text,
                    ApiUrl = txtApiUrl.Text,
                    SenderName = txtSenderName.Text,
                    IsActive = chkIsActive.Checked
                };

                var configId = await _smsRepository.SaveSMSConfigurationAsync(config);
                config.ConfigId = configId;
                _currentConfig = config;

                // Reinitialize SMS service with new configuration
                InitializeSMSService();

                MessageBox.Show("SMS configuration saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnViewLogs_Click(object sender, EventArgs e)
        {
            var logsForm = new SMSLogsForm();
            logsForm.ShowDialog();
        }
    }
}