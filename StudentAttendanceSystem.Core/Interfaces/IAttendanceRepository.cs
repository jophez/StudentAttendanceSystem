using StudentAttendanceSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentAttendanceSystem.Core.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<bool> RecordAttendanceAsync(int studentId, AttendanceType type, string? notes = null);
        Task<List<AttendanceRecord>> GetAttendanceByStudentIdAsync(int studentId);
        Task<StudentAttendanceStatus?> GetStudentAttendanceStatusAsync(int studentId);
        Task<AttendanceValidationResult> ValidateAttendanceActionAsync(int studentId, AttendanceType proposedType);
    }
}
