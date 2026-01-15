using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentAttendanceSystem.Core.Models
{
    public class StudentAttendanceStatus
    {
        public int StudentId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime? LastTimeStamp { get; set; }
    }
}
