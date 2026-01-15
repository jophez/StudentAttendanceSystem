using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Core.RFID;

namespace StudentAttendanceSystem.WinForms.Forms
{
    public partial class RFIDConfigurationForm : Form
    {
        private IRFIDReader? _rfidReader;
        private GroupBox gbReaderInfo;
        private Label lblReaderStatus;
        private Label lblReaderName;
        private Label lblConnectionStatus;
        private Button btnInitialize;
        private Button btnStartReading;
        private Button btnStopReading;
        private Button btnTest;
        private Button btnClose;
        private GroupBox gbTestArea;
        private Label lblLastCardRead;
        private Label lblReadCount;
        private TextBox txtCardData;
        private ListBox lstReadHistory;
        private System.Windows.Forms.Timer statusTimer;
        private int _readCount = 0;

        private bool stopReading = false;
        public RFIDConfigurationForm()
        {
            InitializeComponent();
            SetupRFIDReader();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "RFID Reader Configuration";
            this.Size = new Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Reader Information Group
            gbReaderInfo = new GroupBox
            {
                Text = "RFID Reader Information",
                Location = new Point(20, 20),
                Size = new Size(540, 120),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblReaderName = new Label
            {
                Text = "Reader: Not Connected",
                Location = new Point(15, 25),
                Size = new Size(400, 20),
                Font = new Font("Arial", 9)
            };

            lblConnectionStatus = new Label
            {
                Text = "Status: Disconnected",
                Location = new Point(15, 50),
                Size = new Size(400, 20),
                Font = new Font("Arial", 9),
                ForeColor = Color.Red
            };

            lblReaderStatus = new Label
            {
                Text = "Ready to initialize RFID reader",
                Location = new Point(15, 75),
                Size = new Size(500, 20),
                Font = new Font("Arial", 9),
                ForeColor = Color.Blue
            };

            gbReaderInfo.Controls.AddRange(new Control[] { lblReaderName, lblConnectionStatus, lblReaderStatus });

            // Control Buttons
            btnInitialize = new Button
            {
                Text = "Initialize Reader",
                Location = new Point(20, 160),
                Size = new Size(120, 35),
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnInitialize.Click += BtnInitialize_Click;

            btnStartReading = new Button
            {
                Text = "Start Reading",
                Location = new Point(150, 160),
                Size = new Size(120, 35),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Enabled = false
            };
            btnStartReading.Click += BtnStartReading_Click;

            btnStopReading = new Button
            {
                Text = "Stop Reading",
                Location = new Point(280, 160),
                Size = new Size(120, 35),
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Enabled = false
            };
            btnStopReading.Click += BtnStopReading_Click;

            btnTest = new Button
            {
                Text = "Test Card",
                Location = new Point(410, 160),
                Size = new Size(80, 35),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Enabled = false
            };
            btnTest.Click += BtnTest_Click;

            // Test Area Group
            gbTestArea = new GroupBox
            {
                Text = "Testing & Monitoring",
                Location = new Point(20, 210),
                Size = new Size(540, 200),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblLastCardRead = new Label
            {
                Text = "Last Card: None",
                Location = new Point(15, 25),
                Size = new Size(300, 20),
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };

            lblReadCount = new Label
            {
                Text = "Cards Read: 0",
                Location = new Point(15, 50),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9)
            };

            var lblCardData = new Label
            {
                Text = "Current Card Data:",
                Location = new Point(15, 75),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9)
            };

            txtCardData = new TextBox
            {
                Location = new Point(140, 73),
                Size = new Size(200, 25),
                Font = new Font("Arial", 9),
                ReadOnly = false,
                BackColor = Color.LightYellow
            };
            txtCardData.KeyPress += txtCardData_KeyPress;

            var lblHistory = new Label
            {
                Text = "Read History:",
                Location = new Point(15, 105),
                Size = new Size(100, 20),
                Font = new Font("Arial", 9)
            };

            lstReadHistory = new ListBox
            {
                Location = new Point(15, 125),
                Size = new Size(500, 60),
                Font = new Font("Consolas", 8)
            };

            gbTestArea.Controls.AddRange(new Control[] { 
                lblLastCardRead, lblReadCount, lblCardData, txtCardData, lblHistory, lstReadHistory 
            });

            // Close Button
            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(480, 420),
                Size = new Size(80, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnClose.Click += BtnClose_Click;

            // Add all controls to form
            this.Controls.AddRange(new Control[] {
                gbReaderInfo, btnInitialize, btnStartReading, btnStopReading, btnTest,
                gbTestArea, btnClose
            });

            this.ResumeLayout(false);
        }

        private void SetupRFIDReader()
        {
            _rfidReader = new USBRFIDReader();
            _rfidReader.CardRead += RfidReader_CardRead;
            _rfidReader.ReadError += RfidReader_ReadError;
            _rfidReader.StatusChanged += RfidReader_StatusChanged;
            // Setup status update timer
            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 1000;
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();
        }
        private void txtCardData_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !string.IsNullOrWhiteSpace(txtCardData.Text) && !stopReading)
            {
                // Simulate a card read when Enter is pressed
                var args = new RFIDReadEventArgs
                {
                    CardId = txtCardData.Text,
                    ReadTime = DateTime.Now,
                    RawData = System.Text.Encoding.UTF8.GetBytes(txtCardData.Text)
                };
                RfidReader_CardRead(_rfidReader, args);
                e.Handled = true; // Prevent the beep sound on Enter key
            }
        }
        private async void BtnInitialize_Click(object sender, EventArgs e)
        {
            try
            {
                btnInitialize.Enabled = false;
                btnInitialize.Text = "Initializing...";

                var success = await _rfidReader!.InitializeAsync();
                
                if (success)
                {
                    btnStartReading.Enabled = true;
                    btnTest.Enabled = true;
                    MessageBox.Show("RFID Reader initialized successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to initialize RFID Reader. Please check the device connection.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing RFID Reader: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnInitialize.Enabled = true;
                btnInitialize.Text = "Initialize Reader";
            }
        }

        private async void BtnStartReading_Click(object sender, EventArgs e)
        {
            try
            {
                var success = await _rfidReader!.StartReadingAsync();
                
                if (success)
                {
                    stopReading = false;
                    txtCardData.ReadOnly = false;
                    txtCardData.Focus();
                    txtCardData.Clear();
                    btnStartReading.Enabled = false;
                    btnStopReading.Enabled = true;
                    MessageBox.Show("RFID Reader started. Please scan a card to test.", "Started", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting RFID Reader: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnStopReading_Click(object sender, EventArgs e)
        {
            try
            {
                var success = await _rfidReader!.StopReadingAsync();
                
                if (success)
                {
                    stopReading = true;
                    btnStartReading.Enabled = true;
                    btnStopReading.Enabled = false;
                    txtCardData.ReadOnly = true;
                    MessageBox.Show("RFID Reader stopped.", "Stopped", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping RFID Reader: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            if (!stopReading)
            {
                // Simulate a card read for testing purposes
                var testCardId = "TEST" + DateTime.Now.ToString("HHmmss");
                var args = new RFIDReadEventArgs
                {
                    CardId = testCardId,
                    ReadTime = DateTime.Now,
                    RawData = System.Text.Encoding.UTF8.GetBytes(testCardId)
                };
                RfidReader_CardRead(_rfidReader, args);
                MessageBox.Show($"Test card simulated: {testCardId}", "Test Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCardData.Focus();
                txtCardData.SelectAll();
            }
        }

        private void RfidReader_CardRead(object? sender, RFIDReadEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => RfidReader_CardRead(sender, e));
                return;
            }

            _readCount++;
            lblLastCardRead.Text = $"Last Card: {e.CardId}";
            lblReadCount.Text = $"Cards Read: {_readCount}";
            txtCardData.Text = e.CardId;
            txtCardData.SelectAll();
            var historyEntry = $"{e.ReadTime:HH:mm:ss} - {e.CardId}";
            lstReadHistory.Items.Insert(0, historyEntry);
            
            if (lstReadHistory.Items.Count > 10)
            {
                lstReadHistory.Items.RemoveAt(10);
            }

            // Flash the form to indicate successful read
            this.BackColor = Color.LightGreen;
            var flashTimer = new System.Windows.Forms.Timer();
            flashTimer.Interval = 200;
            flashTimer.Tick += (s, args) =>
            {
                this.BackColor = Color.White;
                flashTimer.Stop();
                flashTimer.Dispose();
            };
            flashTimer.Start();
        }

        private void RfidReader_ReadError(object? sender, RFIDErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => RfidReader_ReadError(sender, e));
                return;
            }

            lblReaderStatus.Text = $"Error: {e.ErrorMessage}";
            lblReaderStatus.ForeColor = Color.Red;
            
            MessageBox.Show($"RFID Reader Error: {e.ErrorMessage}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void RfidReader_StatusChanged(object? sender, RFIDStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => RfidReader_StatusChanged(sender, e));
                return;
            }

            lblReaderStatus.Text = e.Message;
            
            switch (e.Status)
            {
                case RFIDReaderStatus.Connected:
                    lblConnectionStatus.Text = "Status: Connected";
                    lblConnectionStatus.ForeColor = Color.Green;
                    lblReaderName.Text = $"Reader: {_rfidReader?.ReaderName}";
                    break;
                case RFIDReaderStatus.Disconnected:
                    lblConnectionStatus.Text = "Status: Disconnected";
                    lblConnectionStatus.ForeColor = Color.Red;
                    break;
                case RFIDReaderStatus.Reading:
                    lblConnectionStatus.Text = "Status: Reading";
                    lblConnectionStatus.ForeColor = Color.Blue;
                    break;
                case RFIDReaderStatus.Error:
                    lblConnectionStatus.Text = "Status: Error";
                    lblConnectionStatus.ForeColor = Color.Red;
                    lblReaderStatus.ForeColor = Color.Red;
                    break;
                default:
                    lblReaderStatus.ForeColor = Color.Blue;
                    break;
            }
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            // Update connection status periodically
            if (_rfidReader != null)
            {
                if (_rfidReader.IsConnected && lblConnectionStatus.ForeColor == Color.Red)
                {
                    lblConnectionStatus.Text = "Status: Connected";
                    lblConnectionStatus.ForeColor = Color.Green;
                }
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            statusTimer?.Stop();
            statusTimer?.Dispose();
            
            _rfidReader?.StopReadingAsync();
            _rfidReader?.Dispose();
            
            base.OnFormClosed(e);
        }
    }
}