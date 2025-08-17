namespace StudentAttendanceSystem.Core.Models
{
    public class SMSLog
    {
        public int LogId { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public SMSStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentDate { get; set; }
        public string? ProviderResponse { get; set; }
    }

    public enum SMSStatus
    {
        Pending = 1,
        Sent = 2,
        Failed = 3,
        Delivered = 4
    }
}