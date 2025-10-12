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
        private readonly IAttendanceRepository _attendanceRepository;

        public RFIDService(
            Func<string, Task<Student?>> getStudentByRFID,
            Func<int, AttendanceType, Task<bool>> recordAttendance,
            IAttendanceRepository attendanceRepository)
        {
            _getStudentByRFID = getStudentByRFID;
            _recordAttendance = recordAttendance;
            _attendanceRepository = attendanceRepository;
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
                    try
                    {
                        // Use enhanced database-driven attendance type determination
                        var attendanceType = await DetermineAttendanceTypeAsync(student);

                        // Validate the proposed attendance action
                        var validationResult = await _attendanceRepository.ValidateAttendanceActionAsync(
                            student.StudentId, attendanceType);

                        if (!validationResult.IsValid)
                        {
                            OnRFIDError(new RFIDErrorEventArgs
                            {
                                ErrorMessage = validationResult.ValidationMessage
                            });
                            return;
                        }

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
                    catch (InvalidOperationException ioe)
                    {
                        // Business rule violation
                        OnRFIDError(new RFIDErrorEventArgs
                        {
                            ErrorMessage = ioe.Message
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

        private async Task<AttendanceType> DetermineAttendanceTypeAsync(Student student)
        {
            try
            {
                // Get current attendance status from database via repository
                var status = await _attendanceRepository.GetStudentAttendanceStatusAsync(student.StudentId);

                if (status != null)
                {
                    return ApplyAttendanceBusinessRules(status);
                }
                else
                {
                    // No attendance record found, apply fallback logic
                    return ApplyTimeBasedFallback();
                }
            }
            catch (Exception ex)
            {
                // Log error and fall back to improved time-based logic
                OnRFIDError(new RFIDErrorEventArgs
                {
                    ErrorMessage = $"Database error determining attendance type: {ex.Message}. Using fallback logic.",
                    Exception = ex
                });

                return ApplyTimeBasedFallback();
            }
        }

        // BUSINESS RULES - Applied based on database status
        private AttendanceType ApplyAttendanceBusinessRules(StudentAttendanceStatus status)
        {
            // Business Rule 1: If student is OUT, next scan is TimeIn
            if (status.CurrentStatus == "OUT")
            {
                return AttendanceType.IN;
            }

            // Business Rule 2: If student is IN, next scan is TimeOut
            if (status.CurrentStatus == "IN")
            {
                return AttendanceType.OUT;
            }

            // Default fallback
            return AttendanceType.IN;
        }

        // IMPROVED FALLBACK LOGIC - More sophisticated than original 12-hour rule
        private AttendanceType ApplyTimeBasedFallback()
        {
            var now = DateTime.Now;
            var timeOfDay = now.TimeOfDay;

            // More sophisticated time-based logic as fallback
            var morningStart = new TimeSpan(6, 0, 0);    // 6:00 AM
            var morningEnd = new TimeSpan(10, 0, 0);     // 10:00 AM
            var afternoonStart = new TimeSpan(13, 0, 0); // 1:00 PM  
            var eveningEnd = new TimeSpan(18, 0, 0);     // 6:00 PM

            if (timeOfDay >= morningStart && timeOfDay <= morningEnd)
            {
                return AttendanceType.IN; // Morning arrival window
            }
            else if (timeOfDay >= afternoonStart && timeOfDay <= eveningEnd)
            {
                return AttendanceType.OUT; // Afternoon/Evening departure window
            }
            else
            {
                // Off-hours - default to TimeIn for safety
                return AttendanceType.IN;
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