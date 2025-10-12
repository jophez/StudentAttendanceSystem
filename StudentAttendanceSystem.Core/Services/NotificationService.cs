using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Core.Models;

namespace StudentAttendanceSystem.Core.Services
{
    public class NotificationService
    {
        private readonly ISMSService _smsService;
        private readonly Func<Task<SMSConfiguration?>> _getSMSConfig;

        public event EventHandler<NotificationEventArgs>? NotificationSent;
        public event EventHandler<NotificationErrorEventArgs>? NotificationError;

        public NotificationService(
            ISMSService smsService,
            Func<Task<SMSConfiguration?>> getSMSConfig)
        {
            _smsService = smsService;
            _getSMSConfig = getSMSConfig;
        }

        public async Task<bool> SendAttendanceNotificationAsync(Student student, AttendanceType attendanceType, DateTime scanTime)
        {
            try
            {
                if (student.Guardian == null)
                {
                    OnNotificationError($"No guardian found for student {student.FirstName} {student.LastName}");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(student.Guardian.CellPhone))
                {
                    OnNotificationError($"No phone number found for guardian of {student.FirstName} {student.LastName}");
                    return false;
                }

                // Check if SMS service is configured
                var config = await _getSMSConfig();
                if (config == null || !config.IsActive)
                {
                    OnNotificationError("SMS service is not configured or inactive");
                    return false;
                }

                // Create message based on attendance type
                var message = CreateAttendanceMessage(student, attendanceType, scanTime);

                // Send SMS
                var result = await _smsService.SendSMSAsync(
                    student.Guardian.CellPhone, 
                    message, 
                    student.StudentId);

                if (result.Success)
                {
                    OnNotificationSent(new NotificationEventArgs
                    {
                        Student = student,
                        NotificationType = NotificationType.Attendance,
                        AttendanceType = attendanceType,
                        PhoneNumber = student.Guardian.CellPhone,
                        Message = message,
                        SentAt = result.SentAt,
                        MessageId = result.MessageId
                    });
                    return true;
                }
                else
                {
                    OnNotificationError($"Failed to send SMS to {student.Guardian.CellPhone}: {result.ErrorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnNotificationError($"Error sending attendance notification: {ex.Message}");
                return false;
            }
        }

        public async Task<NotificationBulkResult> SendBulkAnnouncementAsync(string message, List<Student>? specificStudents = null)
        {
            var result = new NotificationBulkResult();
            
            try
            {
                // Check if SMS service is configured
                var config = await _getSMSConfig();
                if (config == null || !config.IsActive)
                {
                    result.ErrorMessage = "SMS service is not configured or inactive";
                    return result;
                }

                // Get recipients
                var recipients = new List<Student>();
                if (specificStudents != null && specificStudents.Any())
                {
                    recipients = specificStudents.Where(s => s.Guardian != null && 
                        !string.IsNullOrWhiteSpace(s.Guardian.CellPhone)).ToList();
                }
                else
                {
                    // This would typically load all active students from repository
                    // For now, we'll work with the provided students
                    recipients = specificStudents ?? new List<Student>();
                }

                result.TotalRecipients = recipients.Count;

                // Send SMS to each recipient
                var smsRequests = recipients.Select(student => new SMSRequest
                {
                    PhoneNumber = student.Guardian!.CellPhone,
                    Message = $"Dear Parent/Guardian of {student.FirstName} {student.LastName}, {message} - School Attendance System",
                    StudentId = student.StudentId
                }).ToList();

                if (smsRequests.Any())
                {
                    var smsResult = await _smsService.SendBulkSMSAsync(smsRequests);
                    result.Success = smsResult.Success;
                    result.SentCount = smsResult.Success ? smsRequests.Count : 0;
                    result.FailedCount = result.TotalRecipients - result.SentCount;
                    result.ErrorMessage = smsResult.ErrorMessage;
                }
                else
                {
                    result.ErrorMessage = "No valid recipients found";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error sending bulk announcement: {ex.Message}";
            }

            return result;
        }

        private string CreateAttendanceMessage(Student student, AttendanceType attendanceType, DateTime scanTime)
        {
            var typeText = attendanceType == AttendanceType.IN ? "arrived at" : "left";
            var timeText = scanTime.ToString("HH:mm");
            var dateText = scanTime.ToString("yyyy-MM-dd");

            return $"Your child {student.FirstName} {student.LastName} has {typeText} school at {timeText} on {dateText}. - School Attendance System";
        }

        private void OnNotificationSent(NotificationEventArgs args)
        {
            NotificationSent?.Invoke(this, args);
        }

        private void OnNotificationError(string errorMessage)
        {
            NotificationError?.Invoke(this, new NotificationErrorEventArgs
            {
                ErrorMessage = errorMessage,
                Timestamp = DateTime.Now
            });
        }
    }

    public class NotificationEventArgs : EventArgs
    {
        public Student Student { get; set; } = null!;
        public NotificationType NotificationType { get; set; }
        public AttendanceType? AttendanceType { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string MessageId { get; set; } = string.Empty;
    }

    public class NotificationErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Exception? Exception { get; set; }
    }

    public class NotificationBulkResult
    {
        public bool Success { get; set; }
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.Now;
    }

    public enum NotificationType
    {
        Attendance = 1,
        Announcement = 2,
        Emergency = 3,
        Reminder = 4
    }
}