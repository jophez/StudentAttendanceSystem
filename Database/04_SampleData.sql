USE StudentAttendanceDB;
GO

-- Insert default Administrator user (password: admin123)
INSERT INTO Users (Username, PasswordHash, FirstName, LastName, Email, Role, IsActive)
VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'System', 'Administrator', 'admin@school.com', 1, 1); --admin123

-- Insert sample regular user (password: user123)
INSERT INTO Users (Username, PasswordHash, FirstName, LastName, Email, Role, IsActive)
VALUES ('user1', '8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918', 'John', 'Doe', 'john.doe@school.com', 2, 1);

-- Insert sample guardians
INSERT INTO Guardians (FirstName, LastName, CellPhone, Email, IsActive)
VALUES 
('Maria', 'Santos', '+639123456789', 'maria.santos@email.com', 1),
('Juan', 'Cruz', '+639987654321', 'juan.cruz@email.com', 1),
('Rosa', 'Garcia', '+639456789123', 'rosa.garcia@email.com', 1);

-- Insert sample students
INSERT INTO Students (StudentNumber, FirstName, MiddleName, LastName, CellPhone, Email, StreetAddress, Barangay, Municipality, City, GuardianId, IsActive)
VALUES 
('2024-001', 'Pedro', 'Miguel', 'Santos', '+639111111111', 'pedro.santos@student.com', '123 Main St', 'Poblacion', 'Sample Municipality', 'Sample City', 1, 1),
('2024-002', 'Ana', 'Rose', 'Cruz', '+639222222222', 'ana.cruz@student.com', '456 Oak Ave', 'San Jose', 'Sample Municipality', 'Sample City', 2, 1),
('2024-003', 'Luis', 'Antonio', 'Garcia', '+639333333333', 'luis.garcia@student.com', '789 Pine Rd', 'Santa Maria', 'Sample Municipality', 'Sample City', 3, 1);

-- Insert SMS Configuration for Semaphore
INSERT INTO SMSConfiguration (ProviderName, ApiKey, ApiUrl, SenderName, IsActive)
VALUES ('Semaphore', 'your-semaphore-api-key-here', 'https://api.semaphore.co/api/v4/messages', 'SchoolSMS', 1);

-- Insert RFID Configuration
INSERT INTO RFIDConfiguration (ReaderPort, BaudRate, Timeout, IsActive)
VALUES ('COM3', 9600, 5000, 1);

GO