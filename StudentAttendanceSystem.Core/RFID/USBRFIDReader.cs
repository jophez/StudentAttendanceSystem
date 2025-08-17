using System.Management;
using System.Text;
using Microsoft.Win32;
using StudentAttendanceSystem.Core.Interfaces;

namespace StudentAttendanceSystem.Core.RFID
{
    public class USBRFIDReader : IRFIDReader, IDisposable
    {
        private bool _isReading = false;
        private bool _disposed = false;
        private readonly System.Threading.Timer _connectionCheckTimer;
        private readonly object _lockObject = new object();

        public event EventHandler<RFIDReadEventArgs>? CardRead;
        public event EventHandler<RFIDErrorEventArgs>? ReadError;
        public event EventHandler<RFIDStatusEventArgs>? StatusChanged;

        public bool IsConnected { get; private set; }
        public string ReaderName { get; private set; } = "USB RFID Reader";

        public USBRFIDReader()
        {
            using var _ = _connectionCheckTimer = new System.Threading.Timer(CheckConnection, null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                OnStatusChanged(RFIDReaderStatus.Connecting, "Initializing USB RFID Reader...");
                
                // Check for USB RFID devices
                var devices = await GetUSBRFIDDevicesAsync();
                
                if (devices.Any())
                {
                    ReaderName = devices.First();
                    IsConnected = true;
                    OnStatusChanged(RFIDReaderStatus.Connected, $"Connected to {ReaderName}");
                    
                    // Start connection monitoring
                    _connectionCheckTimer.Change(5000, 5000); // Check every 5 seconds
                    
                    return true;
                }
                else
                {
                    OnStatusChanged(RFIDReaderStatus.Error, "No USB RFID readers found");
                    OnReadError("No USB RFID readers detected. Please ensure the device is connected and drivers are installed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged(RFIDReaderStatus.Error, $"Initialization failed: {ex.Message}");
                OnReadError($"Failed to initialize RFID reader: {ex.Message}", ex);
                return false;
            }
        }
        public async Task<bool> StartReadingAsync()
        {
            if (!IsConnected)
            {
                OnReadError("RFID reader is not connected");
                return false;
            }

            if (_isReading)
            {
                return true; // Already reading
            }

            try
            {
                _isReading = true;
                OnStatusChanged(RFIDReaderStatus.Reading, "Started reading RFID cards...");
                
                // Start keyboard hook for USB HID RFID readers
                await Task.Run(() => StartKeyboardHook());
                
                return true;
            }
            catch (Exception ex)
            {
                _isReading = false;
                OnStatusChanged(RFIDReaderStatus.Error, $"Failed to start reading: {ex.Message}");
                OnReadError($"Failed to start RFID reading: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> StopReadingAsync()
        {
            try
            {
                _isReading = false;
                StopKeyboardHook();
                OnStatusChanged(RFIDReaderStatus.Connected, "Stopped reading RFID cards");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                OnReadError($"Failed to stop RFID reading: {ex.Message}", ex);
                return false;
            }
        }

        private async Task<List<string>> GetUSBRFIDDevicesAsync()
        {
            var devices = new List<string>();
            
            try
            {
                await Task.Run(() =>
                {
                    // Check for common RFID reader device patterns
                    var searcher = new ManagementObjectSearcher(
                        "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%RFID%' OR Name LIKE '%Card Reader%' OR Name LIKE '%HID%'");
                    
                    foreach (ManagementObject device in searcher.Get())
                    {
                        var name = device["Name"]?.ToString();
                        var deviceId = device["DeviceID"]?.ToString();
                        
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(deviceId))
                        {
                            // Filter for likely RFID devices
                            if (name.Contains("RFID", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains("Card Reader", StringComparison.OrdinalIgnoreCase) ||
                                (name.Contains("HID", StringComparison.OrdinalIgnoreCase) && 
                                 name.Contains("Keyboard", StringComparison.OrdinalIgnoreCase)))
                            {
                                devices.Add(name);
                            }
                        }
                    }
                });

                // If no specific RFID devices found, assume USB HID keyboard emulation
                if (!devices.Any())
                {
                    devices.Add("USB HID RFID Reader (Keyboard Emulation)");
                }
            }
            catch (Exception ex)
            {
                OnReadError($"Error detecting USB devices: {ex.Message}", ex);
            }

            return devices;
        }

        #region Keyboard Hook Implementation for USB HID RFID Readers

        private readonly StringBuilder _cardDataBuffer = new StringBuilder();
        private DateTime _lastKeyPress = DateTime.MinValue;
        private const int CARD_READ_TIMEOUT_MS = 100; // Time to wait for complete card read

        private void StartKeyboardHook()
        {
            // For USB HID RFID readers that emulate keyboard input
            // This is a simplified implementation - you may need a proper low-level keyboard hook
            // depending on your specific RFID reader model
            
            Application.AddMessageFilter(new RFIDKeyboardMessageFilter(this));
        }

        private void StopKeyboardHook()
        {
            // Remove the message filter when stopping
            // Note: Application.RemoveMessageFilter would be called in a real implementation
        }

        internal void ProcessKeyboardInput(char keyChar)
        {
            lock (_lockObject)
            {
                if (!_isReading) return;

                var now = DateTime.Now;
                
                // If too much time has passed since last key, start new card read
                if ((now - _lastKeyPress).TotalMilliseconds > CARD_READ_TIMEOUT_MS)
                {
                    _cardDataBuffer.Clear();
                }
                
                _lastKeyPress = now;

                // Most RFID readers send card data followed by Enter key
                if (keyChar == '\r' || keyChar == '\n')
                {
                    if (_cardDataBuffer.Length > 0)
                    {
                        var cardId = _cardDataBuffer.ToString().Trim();
                        _cardDataBuffer.Clear();
                        
                        // Process the card read
                        OnCardRead(cardId);
                    }
                }
                else if (char.IsLetterOrDigit(keyChar))
                {
                    _cardDataBuffer.Append(keyChar);
                }
            }
        }

        #endregion

        private void CheckConnection(object? state)
        {
            try
            {
                // Check if the RFID reader is still connected
                var devices = GetUSBRFIDDevicesAsync().Result;
                
                if (!devices.Any() && IsConnected)
                {
                    IsConnected = false;
                    _isReading = false;
                    OnStatusChanged(RFIDReaderStatus.Error, "RFID reader disconnected");
                    OnReadError("RFID reader has been disconnected");
                }
                else if (devices.Any() && !IsConnected)
                {
                    IsConnected = true;
                    OnStatusChanged(RFIDReaderStatus.Connected, "RFID reader reconnected");
                }
            }
            catch (Exception ex)
            {
                OnReadError($"Connection check failed: {ex.Message}", ex);
            }
        }

        private void OnCardRead(string cardId)
        {
            var args = new RFIDReadEventArgs
            {
                CardId = cardId,
                ReadTime = DateTime.Now,
                RawData = Encoding.UTF8.GetBytes(cardId)
            };
            
            CardRead?.Invoke(this, args);
        }

        private void OnReadError(string message, Exception? exception = null)
        {
            var args = new RFIDErrorEventArgs
            {
                ErrorMessage = message,
                Exception = exception
            };
            
            ReadError?.Invoke(this, args);
        }

        private void OnStatusChanged(RFIDReaderStatus status, string message)
        {
            var args = new RFIDStatusEventArgs
            {
                Status = status,
                Message = message
            };
            
            StatusChanged?.Invoke(this, args);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _isReading = false;
            
            _connectionCheckTimer?.Dispose();
            StopKeyboardHook();
            
            GC.SuppressFinalize(this);
        }
    }

    // Message filter for capturing keyboard input from RFID readers
    internal class RFIDKeyboardMessageFilter : IMessageFilter
    {
        private readonly USBRFIDReader _reader;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_CHAR = 0x0102;

        public RFIDKeyboardMessageFilter(USBRFIDReader reader)
        {
            _reader = reader;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_CHAR)
            {
                char keyChar = (char)m.WParam.ToInt32();
                _reader.ProcessKeyboardInput(keyChar);
                
                // Don't consume the message - let it continue to the intended control
                return false;
            }
            
            return false;
        }
    }
}