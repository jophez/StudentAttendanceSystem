using System.Data;
using System.Data.SqlClient;
using StudentAttendanceSystem.Core.Models;

namespace StudentAttendanceSystem.Data.Repositories
{
    public class AttendanceRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public AttendanceRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> RecordAttendanceAsync(int studentId, AttendanceType type, string? notes = null)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_RecordAttendance", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", studentId);
            command.Parameters.AddWithValue("@Type", (int)type);
            command.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<AttendanceRecord>> GetAttendanceByStudentIdAsync(int studentId)
        {
            var records = new List<AttendanceRecord>();
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetStudentAttendanceToday", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", studentId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                DateTime dateTime = DateTime.Parse("1900/01/01");
                records.Add(new AttendanceRecord
                {
                    AttendanceId = reader.GetInt32("AttendanceId"),
                    StudentId = reader.GetInt32("StudentId"),
                    TimeIn = reader.IsDBNull("TimeIn") ? dateTime : reader.GetDateTime("TimeIn"),
                    TimeOut = reader.IsDBNull("TimeOut") ? null : reader.GetDateTime("TimeOut"),
                    Type = (AttendanceType)reader.GetInt32("Type"),
                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                    TimeStamp = reader.GetDateTime("RecordedDate")
                });
            }

            return records;
        }
    }
}