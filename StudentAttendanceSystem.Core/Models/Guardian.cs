namespace StudentAttendanceSystem.Core.Models
{
    public class Guardian
    {
        public int GuardianId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CellPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
    }
}