using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Core.Models;
using StudentAttendanceSystem.Core.RFID;

namespace StudentAttendanceSystem.Core.Services
{
    public class RFIDService : IDisposable
    {
        private IRFIDReader? _rfidReader;
        private bool _disposed = false;

        public event EventHandler<StudentAttendanceEventArgs>? StudentScanned;
        public event EventHandler<RFIDErrorEventArgs>? RFIDError;
        public event EventHandler<RFIDStatusEventArgs>? StatusChanged;

        public bool IsReading { get; private set; }
        public bool IsConnected => _rfidReader?.IsConnected ?? false;

        private readonly Func<string, Task<Student?>> _getStudentByRFID;
        private readonly Func<int, AttendanceType, Task<bool>> _recordAttendance;

        public RFIDService(
            Func<string, Task<Student?>> getStudentByRFID,
            Func<int, AttendanceType, Task<bool>> recordAttendance)
        {
            _getStudentByRFID = getStudentByRFID;
            _recordAttendance = recordAttendance;
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _rfidReader = new USBRFIDReader();
                
                _rfidReader.CardRead += OnCardRead;
                _rfidReader.ReadError += RFIDError;
                _rfidReader.StatusChanged += OnStatusChanged;

                return await _rfidReader.InitializeAsync();
            }
            catch (Exception ex)
            {
                OnRFIDError(new RFIDErrorEventArgs 
                { 
                    ErrorMessage = $"Failed to initialize RFID service: {ex.Message}",
                    Exception = ex 
                });
                return false;
            }
        }

        public async Task<bool> StartReadingAsync()
        {
            if (_rfidReader == null)
            {
                OnRFIDError(new RFIDErrorEventArgs 
                { 
                    ErrorMessage = "RFID reader not initialized" 
                });
                return false;
            }

            var success = await _rfidReader.StartReadingAsync();
            if (success)
            {
                IsReading = true;
            }
            return success;
        }

        public async Task<bool> StopReadingAsync()
        {
            if (_rfidReader == null) return true;

            var success = await _rfidReader.StopReadingAsync();
            if (success)
            {
                IsReading = false;
            }
            return success;
        }

        private async void OnCardRead(object? sender, RFIDReadEventArgs e)
        {
            try
            {
                // Look up student by RFID code
                var student = await _getStudentByRFID(e.CardId);
                
                if (student != null)
                {
                    // Determine if this is time in or time out
                    var attendanceType = DetermineAttendanceType(student);
                    
                    // Record attendance
                    var success = await _recordAttendance(student.StudentId, attendanceType);
                    
                    if (success)
                    {
                        OnStudentScanned(new StudentAttendanceEventArgs
                        {
                            Student = student,
                            AttendanceType = attendanceType,
                            ScanTime = e.ReadTime,
                            RFIDCode = e.CardId,
                            Success = true
                        });
                    }
                    else
                    {
                        OnStudentScanned(new StudentAttendanceEventArgs
                        {
                            Student = student,
                            AttendanceType = attendanceType,
                            ScanTime = e.ReadTime,
                            RFIDCode = e.CardId,
                            Success = false,
                            ErrorMessage = "Failed to record attendance"
                        });
                    }
                }
                else
                {
                    OnRFIDError(new RFIDErrorEventArgs
                    {
                        ErrorMessage = $"Unknown RFID card: {e.CardId}. Student not found."
                    });
                }
            }
            catch (Exception ex)
            {
                OnRFIDError(new RFIDErrorEventArgs
                {
                    ErrorMessage = $"Error processing RFID scan: {ex.Message}",
                    Exception = ex
                });
            }
        }

        private AttendanceType DetermineAttendanceType(Student student)
        {
            // Simple logic: alternate between time in and time out
            // In a real system, you might check the last attendance record
            // to determine if the student is currently "in" or "out"
            var now = DateTime.Now;
            
            // If it's before noon, assume time in, otherwise time out
            if (now.Hour < 12)
            {
                return AttendanceType.TimeIn;
            }
            else
            {
                return AttendanceType.TimeOut;
            }
        }

        private void OnStudentScanned(StudentAttendanceEventArgs args)
        {
            StudentScanned?.Invoke(this, args);
        }

        private void OnRFIDError(RFIDErrorEventArgs args)
        {
            RFIDError?.Invoke(this, args);
        }

        private void OnStatusChanged(object? sender, RFIDStatusEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            IsReading = false;

            if (_rfidReader != null)
            {
                _rfidReader.CardRead -= OnCardRead;
                _rfidReader.ReadError -= RFIDError;
                _rfidReader.StatusChanged -= OnStatusChanged;
                _rfidReader.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }

    public class StudentAttendanceEventArgs : EventArgs
    {
        public Student Student { get; set; } = null!;
        public AttendanceType AttendanceType { get; set; }
        public DateTime ScanTime { get; set; }
        public string RFIDCode { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}