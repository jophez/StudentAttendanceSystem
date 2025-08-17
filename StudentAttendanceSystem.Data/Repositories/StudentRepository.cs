using System.Data;
using System.Data.SqlClient;
using StudentAttendanceSystem.Core.Models;

namespace StudentAttendanceSystem.Data.Repositories
{
    public class StudentRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public StudentRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            var students = new List<Student>();
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetAllStudents", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var student = new Student
                {
                    StudentId = reader.GetInt32("StudentId"),
                    StudentNumber = reader.GetString("StudentNumber"),
                    FirstName = reader.GetString("FirstName"),
                    MiddleName = reader.GetString("MiddleName"),
                    LastName = reader.GetString("LastName"),
                    CellPhone = reader.GetString("CellPhone"),
                    Email = reader.GetString("Email"),
                    ImagePath = reader.IsDBNull("ImagePath") ? null : reader.GetString("ImagePath"),
                    StreetAddress = reader.GetString("StreetAddress"),
                    Barangay = reader.GetString("Barangay"),
                    Municipality = reader.GetString("Municipality"),
                    City = reader.GetString("City"),
                    GuardianId = reader.IsDBNull("GuardianId") ? null : reader.GetInt32("GuardianId"),
                    RFIDTag = reader.IsDBNull("RFIDCode") ? null : reader.GetString("RFIDCode"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                };

                if (!reader.IsDBNull("GuardianId"))
                {
                    student.Guardian = new Guardian
                    {
                        GuardianId = reader.GetInt32("GuardianId"),
                        FirstName = reader.GetString("GuardianFirstName"),
                        LastName = reader.GetString("GuardianLastName"),
                        CellPhone = reader.GetString("GuardianCellPhone"),
                        Email = reader.GetString("GuardianEmail")
                    };
                }

                students.Add(student);
            }

            return students;
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetStudentById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", studentId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var student = new Student
                {
                    StudentId = reader.GetInt32("StudentId"),
                    StudentNumber = reader.GetString("StudentNumber"),
                    FirstName = reader.GetString("FirstName"),
                    MiddleName = reader.GetString("MiddleName"),
                    LastName = reader.GetString("LastName"),
                    CellPhone = reader.GetString("CellPhone"),
                    Email = reader.GetString("Email"),
                    ImagePath = reader.IsDBNull("ImagePath") ? null : reader.GetString("ImagePath"),
                    StreetAddress = reader.GetString("StreetAddress"),
                    Barangay = reader.GetString("Barangay"),
                    Municipality = reader.GetString("Municipality"),
                    City = reader.GetString("City"),
                    GuardianId = reader.IsDBNull("GuardianId") ? null : reader.GetInt32("GuardianId"),
                    RFIDTag = reader.IsDBNull("RFIDCode") ? null : reader.GetString("RFIDCode"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                };

                if (!reader.IsDBNull("GuardianId"))
                {
                    student.Guardian = new Guardian
                    {
                        GuardianId = reader.GetInt32("GuardianId"),
                        FirstName = reader.GetString("GuardianFirstName"),
                        LastName = reader.GetString("GuardianLastName"),
                        CellPhone = reader.GetString("GuardianCellPhone"),
                        Email = reader.GetString("GuardianEmail")
                    };
                }

                return student;
            }

            return null;
        }

        public async Task<Student?> GetStudentByRFIDAsync(string rfidCode)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetStudentByRFID", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@RFIDCode", rfidCode);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var student = new Student
                {
                    StudentId = reader.GetInt32("StudentId"),
                    StudentNumber = reader.GetString("StudentNumber"),
                    FirstName = reader.GetString("FirstName"),
                    MiddleName = reader.GetString("MiddleName"),
                    LastName = reader.GetString("LastName"),
                    CellPhone = reader.GetString("CellPhone"),
                    Email = reader.GetString("Email"),
                    ImagePath = reader.IsDBNull("ImagePath") ? null : reader.GetString("ImagePath"),
                    StreetAddress = reader.GetString("StreetAddress"),
                    Barangay = reader.GetString("Barangay"),
                    Municipality = reader.GetString("Municipality"),
                    City = reader.GetString("City"),
                    GuardianId = reader.IsDBNull("GuardianId") ? null : reader.GetInt32("GuardianId"),
                    RFIDTag = reader.IsDBNull("RFIDCode") ? null : reader.GetString("RFIDCode"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                };

                if (!reader.IsDBNull("GuardianId"))
                {
                    student.Guardian = new Guardian
                    {
                        GuardianId = reader.GetInt32("GuardianId"),
                        FirstName = reader.GetString("GuardianFirstName"),
                        LastName = reader.GetString("GuardianLastName"),
                        CellPhone = reader.GetString("GuardianCellPhone"),
                        Email = reader.GetString("GuardianEmail")
                    };
                }

                return student;
            }

            return null;
        }

        public async Task<int> CreateStudentAsync(Student student)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_CreateStudent", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentNumber", student.StudentNumber);
            command.Parameters.AddWithValue("@FirstName", student.FirstName);
            command.Parameters.AddWithValue("@MiddleName", student.MiddleName);
            command.Parameters.AddWithValue("@LastName", student.LastName);
            command.Parameters.AddWithValue("@CellPhone", student.CellPhone);
            command.Parameters.AddWithValue("@Email", student.Email);
            command.Parameters.AddWithValue("@ImagePath", (object?)student.ImagePath ?? DBNull.Value);
            command.Parameters.AddWithValue("@StreetAddress", student.StreetAddress);
            command.Parameters.AddWithValue("@Barangay", student.Barangay);
            command.Parameters.AddWithValue("@Municipality", student.Municipality);
            command.Parameters.AddWithValue("@City", student.City);
            command.Parameters.AddWithValue("@GuardianId", (object?)student.GuardianId ?? DBNull.Value);
            command.Parameters.AddWithValue("@RFIDCode", (object?)student.RFIDTag ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", student.IsActive);

            var outputParam = new SqlParameter("@StudentId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)outputParam.Value;
        }

        public async Task<string> AssignRFIDAsync(int studentId, string? rfidCode = null)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_AssignRFIDToStudent", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", studentId);
            command.Parameters.AddWithValue("@RFIDCode", (object?)rfidCode ?? DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetString("AssignedRFIDCode");
            }

            throw new Exception("Failed to assign RFID code");
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_UpdateStudent", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", student.StudentId);
            command.Parameters.AddWithValue("@StudentNumber", student.StudentNumber);
            command.Parameters.AddWithValue("@FirstName", student.FirstName);
            command.Parameters.AddWithValue("@MiddleName", student.MiddleName);
            command.Parameters.AddWithValue("@LastName", student.LastName);
            command.Parameters.AddWithValue("@CellPhone", student.CellPhone);
            command.Parameters.AddWithValue("@Email", student.Email);
            command.Parameters.AddWithValue("@ImagePath", (object?)student.ImagePath ?? DBNull.Value);
            command.Parameters.AddWithValue("@StreetAddress", student.StreetAddress);
            command.Parameters.AddWithValue("@Barangay", student.Barangay);
            command.Parameters.AddWithValue("@Municipality", student.Municipality);
            command.Parameters.AddWithValue("@City", student.City);
            command.Parameters.AddWithValue("@GuardianId", (object?)student.GuardianId ?? DBNull.Value);
            command.Parameters.AddWithValue("@RFIDCode", (object?)student.RFIDTag ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", student.IsActive);

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

        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_DeleteStudent", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", studentId);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Student>> GetStudentsByGuardianIdAsync(int guardianId)
        {
            var students = new List<Student>();
            using var connection = _dbConnection.GetConnection();
            using var command = new SqlCommand("sp_GetStudentsByGuardianId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@GuardianId", guardianId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var student = new Student
                {
                    StudentId = reader.GetInt32("StudentId"),
                    StudentNumber = reader.GetString("StudentNumber"),
                    FirstName = reader.GetString("FirstName"),
                    MiddleName = reader.GetString("MiddleName"),
                    LastName = reader.GetString("LastName"),
                    CellPhone = reader.GetString("CellPhone"),
                    Email = reader.GetString("Email"),
                    ImagePath = reader.IsDBNull("ImagePath") ? null : reader.GetString("ImagePath"),
                    StreetAddress = reader.GetString("StreetAddress"),
                    Barangay = reader.GetString("Barangay"),
                    Municipality = reader.GetString("Municipality"),
                    City = reader.GetString("City"),
                    GuardianId = reader.GetInt32("GuardianId"),
                    RFIDTag = reader.IsDBNull("RFIDCode") ? null : reader.GetString("RFIDCode"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                };

                students.Add(student);
            }

            return students;
        }
        #region
        //public async Task<Student?> GetStudentByRFIDAsync(string rfidCode)
        //{
        //    using var connection = _dbConnection.GetConnection();
        //    using var command = new SqlCommand("sp_GetStudentByRFID", connection)
        //    {
        //        CommandType = CommandType.StoredProcedure
        //    };

        //    command.Parameters.AddWithValue("@RFIDCode", rfidCode);

        //    await connection.OpenAsync();
        //    using var reader = await command.ExecuteReaderAsync();

        //    if (await reader.ReadAsync())
        //    {
        //        var student = new Student
        //        {
        //            StudentId = reader.GetInt32("StudentId"),
        //            StudentNumber = reader.GetString("StudentNumber"),
        //            FirstName = reader.GetString("FirstName"),
        //            MiddleName = reader.GetString("MiddleName"),
        //            LastName = reader.GetString("LastName"),
        //            CellPhone = reader.GetString("CellPhone"),
        //            Email = reader.GetString("Email"),
        //            ImagePath = reader.IsDBNull("ImagePath") ? null : reader.GetString("ImagePath"),
        //            StreetAddress = reader.GetString("StreetAddress"),
        //            Barangay = reader.GetString("Barangay"),
        //            Municipality = reader.GetString("Municipality"),
        //            City = reader.GetString("City"),
        //            GuardianId = reader.IsDBNull("GuardianId") ? null : reader.GetInt32("GuardianId"),
        //            RFIDCode = reader.GetString("RFIDCode"),
        //            IsActive = reader.GetBoolean("IsActive"),
        //            CreatedDate = reader.GetDateTime("CreatedDate")
        //        };

        //        if (!reader.IsDBNull("GuardianId"))
        //        {
        //            student.Guardian = new Guardian
        //            {
        //                GuardianId = reader.GetInt32("GuardianId"),
        //                FirstName = reader.GetString("GuardianFirstName"),
        //                LastName = reader.GetString("GuardianLastName"),
        //                CellPhone = reader.GetString("GuardianCellPhone"),
        //                Email = reader.GetString("GuardianEmail")
        //            };
        //        }

        //        return student;
        //    }

        //    return null;
        //}
        #endregion
    }
}