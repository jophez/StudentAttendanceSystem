namespace StudentAttendanceSystem.Core.Interfaces
{
    public interface ISMSService
    {
        Task<SMSResult> SendSMSAsync(string phoneNumber, string message, int? studentId = null);
        Task<SMSResult> SendBulkSMSAsync(List<SMSRequest> requests);
        Task<bool> TestConnectionAsync();
        Task<decimal> GetBalanceAsync();
        event EventHandler<SMSStatusEventArgs> SMSStatusChanged;
    }

    public class SMSRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? StudentId { get; set; }
        public string? Reference { get; set; }
    }

    public class SMSResult
    {
        public bool Success { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int? LogId { get; set; }
        public DateTime SentAt { get; set; }
        public decimal? Cost { get; set; }
        public SMSStatus Status { get; set; }
    }

    public class SMSStatusEventArgs : EventArgs
    {
        public string MessageId { get; set; } = string.Empty;
        public SMSStatus Status { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public enum SMSStatus
    {
        Pending = 1,
        Sent = 2,
        Failed = 3,
        Delivered = 4,
        Queued = 5
    }
}