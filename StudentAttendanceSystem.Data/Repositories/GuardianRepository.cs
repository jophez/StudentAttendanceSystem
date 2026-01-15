using System.Data;
using System.Data.SqlClient;
using StudentAttendanceSystem.Core.Models;

namespace StudentAttendanceSystem.Data.Repositories
{
    public class GuardianRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public GuardianRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<Guardian>> GetAllGuardiansAsync()
        {
            var guardians = new List<Guardian>();
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetAllGuardians", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                guardians.Add(new Guardian
                {
                    GuardianId = reader.GetInt32("GuardianId"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    CellPhone = reader.GetString("CellPhone"),
                    Email = reader.GetString("Email"),
                    //Relationship = reader.GetString("Relationship"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                });
            }

            return guardians;
        }

        public async Task<int> CreateGuardianAsync(Guardian guardian)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_CreateGuardian", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@FirstName", guardian.FirstName);
            command.Parameters.AddWithValue("@LastName", guardian.LastName);
            command.Parameters.AddWithValue("@CellPhone", guardian.CellPhone);
            command.Parameters.AddWithValue("@Email", guardian.Email);
            //command.Parameters.AddWithValue("@Relationship", guardian.Relationship);
            command.Parameters.AddWithValue("@IsActive", guardian.IsActive);

            var outputParam = new SqlParameter("@GuardianId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)outputParam.Value;
        }

        public async Task<Guardian?> GetGuardianByIdAsync(int guardianId)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetGuardianById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@GuardianId", guardianId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Guardian
                {
                    GuardianId = reader.GetInt32("GuardianId"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    CellPhone = reader.GetString("CellPhone"),
                    Email = reader.GetString("Email"),
                    //Relationship = reader.GetString("Relationship"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                };
            }

            return null;
        }

        public async Task<bool> UpdateGuardianAsync(Guardian guardian)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_UpdateGuardian", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@GuardianId", guardian.GuardianId);
            command.Parameters.AddWithValue("@FirstName", guardian.FirstName);
            command.Parameters.AddWithValue("@LastName", guardian.LastName);
            command.Parameters.AddWithValue("@CellPhone", guardian.CellPhone);
            command.Parameters.AddWithValue("@Email", guardian.Email);
            //command.Parameters.AddWithValue("@Relationship", guardian.Relationship);
            command.Parameters.AddWithValue("@IsActive", guardian.IsActive);
            
            // Add OUTPUT parameters
            var successParam = new SqlParameter("@Success", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(successParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            // Return the success flag
            return Convert.ToBoolean(successParam.Value);
        }

        public async Task<bool> DeleteGuardianAsync(int guardianId)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_DeleteGuardian", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@GuardianId", guardianId);

            // Add OUTPUT parameters
            var successParam = new SqlParameter("@Success", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(successParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            // Return the success flag
            return Convert.ToBoolean(successParam.Value);
        }
    }
}