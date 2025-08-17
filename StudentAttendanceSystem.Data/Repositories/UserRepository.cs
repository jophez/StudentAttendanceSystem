using System.Data;
using System.Data.SqlClient;
using StudentAttendanceSystem.Core.Models;

namespace StudentAttendanceSystem.Data.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public UserRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<User?> GetUserByCredentialsAsync(string username, string passwordHash)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_ValidateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Username = reader.GetString("Username"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    Email = reader.GetString("Email"),
                    ImagePath = reader.IsDBNull("ImagePath") ? null : reader.GetString("ImagePath"),
                    Role = (UserRole)reader.GetInt32("Role"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                };
            }

            return null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetAllUsers", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Username = reader.GetString("Username"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    Email = reader.GetString("Email"),
                    ImagePath = reader.IsDBNull("ImagePath") ? null : reader.GetString("ImagePath"),
                    Role = (UserRole)reader.GetInt32("Role"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                });
            }

            return users;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetUserById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Username = reader.GetString("Username"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    Email = reader.GetString("Email"),
                    ImagePath = reader.IsDBNull("ImagePath") ? null : reader.GetString("ImagePath"),
                    Role = (UserRole)reader.GetInt32("Role"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                };
            }

            return null;
        }

        public async Task<int> CreateUserAsync(User user, string passwordHash)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_CreateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@ImagePath", (object?)user.ImagePath ?? DBNull.Value);
            command.Parameters.AddWithValue("@Role", (int)user.Role);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);

            var outputParam = new SqlParameter("@UserId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)outputParam.Value;
        }

        public async Task<bool> UpdateUserAsync(User user, string? passwordHash = null)
        {
            using var connection = _dbConnection.GetConnection();
            
            string storedProcedure = string.IsNullOrEmpty(passwordHash) ? "sp_UpdateUser" : "sp_UpdateUserWithPassword";
            using var command = new SqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", user.UserId);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@ImagePath", (object?)user.ImagePath ?? DBNull.Value);
            command.Parameters.AddWithValue("@Role", (int)user.Role);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
            
            if (!string.IsNullOrEmpty(passwordHash))
            {
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
            }

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_DeleteUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}