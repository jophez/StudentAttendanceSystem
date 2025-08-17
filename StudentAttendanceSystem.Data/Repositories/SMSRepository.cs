using System.Data;
using System.Data.SqlClient;
using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Core.Models;
using SMSStatus = StudentAttendanceSystem.Core.Models.SMSStatus;

namespace StudentAttendanceSystem.Data.Repositories
{
    public class SMSRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public SMSRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<SMSConfiguration?> GetActiveSMSConfigurationAsync()
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetSMSConfiguration", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new SMSConfiguration
                {
                    ConfigId = reader.GetInt32("ConfigId"),
                    ProviderName = reader.GetString("ProviderName"),
                    ApiKey = reader.GetString("ApiKey"),
                    ApiUrl = reader.GetString("ApiUrl"),
                    SenderName = reader.GetString("SenderName"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate"),
                    ModifiedDate = reader.IsDBNull("ModifiedDate") ? null : reader.GetDateTime("ModifiedDate")
                };
            }

            return null;
        }

        public async Task<int> SaveSMSConfigurationAsync(SMSConfiguration config)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_SaveSMSConfiguration", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@ConfigId", config.ConfigId == 0 ? DBNull.Value : config.ConfigId);
            command.Parameters.AddWithValue("@ProviderName", config.ProviderName);
            command.Parameters.AddWithValue("@ApiKey", config.ApiKey);
            command.Parameters.AddWithValue("@ApiUrl", config.ApiUrl);
            command.Parameters.AddWithValue("@SenderName", config.SenderName);
            command.Parameters.AddWithValue("@IsActive", config.IsActive);

            var outputParam = new SqlParameter("@OutputConfigId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)outputParam.Value;
        }

        public async Task<int> LogSMSAsync(int? studentId, string phoneNumber, string message, SMSStatus status, string? errorMessage = null, string? providerResponse = null)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_LogSMS", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", (object?)studentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
            command.Parameters.AddWithValue("@Message", message);
            command.Parameters.AddWithValue("@Status", (int)status);
            command.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);
            command.Parameters.AddWithValue("@ProviderResponse", (object?)providerResponse ?? DBNull.Value);

            var outputParam = new SqlParameter("@LogId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)outputParam.Value;
        }

        public async Task<bool> UpdateSMSLogAsync(int logId, SMSStatus status, string? errorMessage = null, string? providerResponse = null)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_UpdateSMSLog", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@LogId", logId);
            command.Parameters.AddWithValue("@Status", (int)status);
            command.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);
            command.Parameters.AddWithValue("@ProviderResponse", (object?)providerResponse ?? DBNull.Value);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<SMSLog>> GetSMSLogsAsync(int? studentId = null, DateTime? fromDate = null, DateTime? toDate = null, int maxRecords = 100)
        {
            var logs = new List<SMSLog>();
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetSMSLogs", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", (object?)studentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@MaxRecords", maxRecords);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(new SMSLog
                {
                    LogId = reader.GetInt32("LogId"),
                    StudentId = reader.GetInt32("StudentId"),
                    PhoneNumber = reader.GetString("PhoneNumber"),
                    Message = reader.GetString("Message"),
                    Status = (SMSStatus)reader.GetInt32("Status"),
                    ErrorMessage = reader.IsDBNull("ErrorMessage") ? null : reader.GetString("ErrorMessage"),
                    SentDate = reader.GetDateTime("SentDate"),
                    ProviderResponse = reader.IsDBNull("ProviderResponse") ? null : reader.GetString("ProviderResponse")
                });
            }

            return logs;
        }

        public async Task<int> GetSMSCountTodayAsync()
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand(@"
                SELECT COUNT(*) FROM SMSLogs 
                WHERE CAST(SentDate AS DATE) = CAST(GETDATE() AS DATE) 
                AND Status = @SentStatus", connection);
            
            command.Parameters.AddWithValue("@SentStatus", (int)SMSStatus.Sent);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }
}