using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentAttendanceSystem.Core.Models
{
    public class AttendanceValidationResult
    {
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
    }
}
