USE StudentAttendanceDB;
GO

-- Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    ImagePath NVARCHAR(500) NULL,
    Role INT NOT NULL DEFAULT 2, -- 1=Administrator, 2=RegularUser
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NULL
);

-- Create index on Username for faster login queries
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Role ON Users(Role);

-- Guardians Table
CREATE TABLE Guardians (
    GuardianId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    CellPhone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NULL
);

-- Create index on Guardian phone for SMS lookups
CREATE INDEX IX_Guardians_CellPhone ON Guardians(CellPhone);

-- Students Table
CREATE TABLE Students (
    StudentId INT IDENTITY(1,1) PRIMARY KEY,
    StudentNumber NVARCHAR(50) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    CellPhone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    ImagePath NVARCHAR(500) NULL,
    StreetAddress NVARCHAR(200) NOT NULL,
    Barangay NVARCHAR(100) NOT NULL,
    Municipality NVARCHAR(100) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    GuardianId INT NULL,
    RFIDCode NVARCHAR(50) NULL UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NULL,
    FOREIGN KEY (GuardianId) REFERENCES Guardians(GuardianId)
);

-- Create indexes for optimized queries
CREATE INDEX IX_Students_StudentNumber ON Students(StudentNumber);
CREATE INDEX IX_Students_RFIDCode ON Students(RFIDCode);
CREATE INDEX IX_Students_GuardianId ON Students(GuardianId);
CREATE INDEX IX_Students_FullName ON Students(LastName, FirstName);

-- Attendance Records Table
CREATE TABLE AttendanceRecords (
    AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    TimeIn DATETIME2 NULL,
    TimeOut DATETIME2 NULL,
    Type INT NOT NULL, -- 1=TimeIn, 2=TimeOut
    Notes NVARCHAR(500) NULL,
    RecordedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId)
);

-- Create indexes for attendance queries
CREATE INDEX IX_AttendanceRecords_StudentId ON AttendanceRecords(StudentId);
CREATE INDEX IX_AttendanceRecords_RecordedDate ON AttendanceRecords(RecordedDate);
CREATE INDEX IX_AttendanceRecords_Type ON AttendanceRecords(Type);

-- SMS Configuration Table
CREATE TABLE SMSConfiguration (
    ConfigId INT IDENTITY(1,1) PRIMARY KEY,
    ProviderName NVARCHAR(100) NOT NULL,
    ApiKey NVARCHAR(500) NOT NULL,
    ApiUrl NVARCHAR(500) NOT NULL,
    SenderName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NULL
);

-- SMS Logs Table
CREATE TABLE SMSLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- 1=Pending, 2=Sent, 3=Failed, 4=Delivered
    ErrorMessage NVARCHAR(1000) NULL,
    SentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ProviderResponse NVARCHAR(2000) NULL,
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId)
);

-- Create index for SMS logs
CREATE INDEX IX_SMSLogs_StudentId ON SMSLogs(StudentId);
CREATE INDEX IX_SMSLogs_SentDate ON SMSLogs(SentDate);
CREATE INDEX IX_SMSLogs_Status ON SMSLogs(Status);

-- RFID Configuration Table
CREATE TABLE RFIDConfiguration (
    ConfigId INT IDENTITY(1,1) PRIMARY KEY,
    ReaderPort NVARCHAR(20) NOT NULL,
    BaudRate INT NOT NULL DEFAULT 9600,
    Timeout INT NOT NULL DEFAULT 5000,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NULL
);

GO

--SELECT 
--    i.name AS IndexName,
--    c.name AS ColumnName,
--    i.has_filter,
--    i.filter_definition
--FROM sys.indexes i
--INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
--INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
--WHERE i.object_id = OBJECT_ID('Students')
--    AND i.name = 'UQ__Students__C319EBB1FFDCEB8A';


--ALTER TABLE Students DROP CONSTRAINT UQ_Students_RFIDCode;

---- Step 2: Create a filtered unique index that allows multiple NULLs
--CREATE UNIQUE INDEX IX_Students_RFIDCode_Unique 
--ON Students (RFIDCode) 
--WHERE RFIDCode IS NOT NULL;