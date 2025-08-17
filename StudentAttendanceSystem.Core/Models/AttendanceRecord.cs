namespace StudentAttendanceSystem.Core.Models
{
    public class AttendanceRecord
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public string? RFIDTag { get; set; }
        public Student? Student { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public AttendanceType Type { get; set; }
        public string? Notes { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public enum AttendanceType
    {
        TimeIn = 1,
        TimeOut = 2
    }
}