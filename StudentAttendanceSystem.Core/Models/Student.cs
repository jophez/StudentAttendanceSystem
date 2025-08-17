namespace StudentAttendanceSystem.Core.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CellPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public string StreetAddress { get; set; } = string.Empty;
        public string Barangay { get; set; } = string.Empty;
        public string Municipality { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int? GuardianId { get; set; }
        public Guardian? Guardian { get; set; }
        public string? RFIDTag { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}