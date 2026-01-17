using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Core.Models;

namespace StudentAttendanceSystem.Data.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly string _durationInMinutes = ConfigurationManager.AppSettings["Duration"].ToString();
        public AttendanceRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(bool, string)> RecordAttendanceAsync(int studentId, AttendanceType type, string? notes = null)
        {
            try
            {
                using var connection = _dbConnection.GetConnection();
                using var command = new SqlCommand("sp_RecordAttendance", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@StudentId", studentId);
                command.Parameters.AddWithValue("@Type", (int)type);
                command.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);
                command.Parameters.AddWithValue("@MinimumMinutes", int.Parse(_durationInMinutes));

                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return (rowsAffected > 0, string.Empty);
            }
            catch (Exception ex)
            {
               (bool, string) result = new (false, ex.Message);
                return result;
            }
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
                    TimeIn = reader.IsDBNull("TimeIn") ? null : reader.GetDateTime("TimeIn"),
                    TimeOut = reader.IsDBNull("TimeOut") ? null : reader.GetDateTime("TimeOut"),
                    Type = (AttendanceType)reader.GetInt32("Type"),
                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                    TimeStamp = reader.GetDateTime("RecordedDate")
                });
            }

            return records;
        }

        public async Task<StudentAttendanceStatus?> GetStudentAttendanceStatusAsync(int studentId)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetStudentAttendanceStatus", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", studentId);

            var currentStatusParam = new SqlParameter("@CurrentStatus", SqlDbType.NVarChar, 10)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(currentStatusParam);

            var lastTimeStampParam = new SqlParameter("@LastTimeStamp", SqlDbType.DateTime)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(lastTimeStampParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            var currentStatus = currentStatusParam.Value?.ToString();
            var lastTimeStamp = lastTimeStampParam.Value as DateTime?;

            if (string.IsNullOrEmpty(currentStatus))
                return null;

            return new StudentAttendanceStatus
            {
                StudentId = studentId,
                CurrentStatus = currentStatus,
                LastTimeStamp = lastTimeStamp
            };
        }

        public async Task<AttendanceValidationResult> ValidateAttendanceActionAsync(int studentId, AttendanceType proposedType)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_ValidateAttendanceAction", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", studentId);
            command.Parameters.AddWithValue("@ProposedAttendanceType", proposedType.ToString());

            var isValidParam = new SqlParameter("@IsValid", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(isValidParam);

            var validationMessageParam = new SqlParameter("@ValidationMessage", SqlDbType.NVarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(validationMessageParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            var isValid = (bool)(isValidParam.Value ?? false);
            var validationMessage = validationMessageParam.Value?.ToString() ?? string.Empty;

            return new AttendanceValidationResult
            {
                IsValid = isValid,
                ValidationMessage = validationMessage
            };
        }
    }
}