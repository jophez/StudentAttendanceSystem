USE StudentAttendanceDB;
GO

-------------------------------------
-- User Management Stored Procedures
-------------------------------------
CREATE OR ALTER PROCEDURE sp_ValidateUser
    @Username NVARCHAR(50),
    @PasswordHash NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Username, FirstName, LastName, Email, ImagePath, Role, IsActive, CreatedDate
    FROM Users
    WHERE Username = @Username 
    AND PasswordHash = @PasswordHash 
    AND IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Username, FirstName, LastName, Email, ImagePath, Role, IsActive, CreatedDate, ModifiedDate
    FROM Users
    WHERE UserId = @UserId
    ORDER BY LastName, FirstName;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Username, FirstName, LastName, Email, ImagePath, Role, IsActive, CreatedDate, ModifiedDate
    FROM Users
    ORDER BY LastName, FirstName;
END
GO

CREATE OR ALTER PROCEDURE sp_CreateUser
    @Username NVARCHAR(50),
    @PasswordHash NVARCHAR(256),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(200),
    @ImagePath NVARCHAR(500) = NULL,
    @Role INT,
    @IsActive BIT,
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO Users (Username, PasswordHash, FirstName, LastName, Email, ImagePath, Role, IsActive)
        VALUES (@Username, @PasswordHash, @FirstName, @LastName, @Email, @ImagePath, @Role, @IsActive);
        
        SET @UserId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateUser
    @UserId INT,
    @Username NVARCHAR(50),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(200),
    @ImagePath NVARCHAR(500) = NULL,
    @Role INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET Username = @Username,
        FirstName = @FirstName,
        LastName = @LastName,
        Email = @Email,
        ImagePath = @ImagePath,
        Role = @Role,
        IsActive = @IsActive,
        ModifiedDate = GETDATE()
    WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE sp_DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET IsActive = 0,
        ModifiedDate = GETDATE()
    WHERE UserId = @UserId;
END
GO

----------------------------------------
-- Student Management Stored Procedures
----------------------------------------
CREATE OR ALTER PROCEDURE sp_GetAllStudents
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT s.StudentId, s.StudentNumber, s.FirstName, s.MiddleName, s.LastName,
           s.CellPhone, s.Email, s.ImagePath, s.StreetAddress, s.Barangay,
           s.Municipality, s.City, s.GuardianId, s.RFIDCode, s.IsActive,
           s.CreatedDate, s.ModifiedDate,
           g.FirstName AS GuardianFirstName, g.LastName AS GuardianLastName,
           g.CellPhone AS GuardianCellPhone, g.Email AS GuardianEmail
    FROM Students s
    LEFT JOIN Guardians g ON s.GuardianId = g.GuardianId
    WHERE s.IsActive = 1
    ORDER BY s.LastName, s.FirstName;
END
GO

CREATE OR ALTER PROCEDURE sp_GetStudentById
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT s.StudentId, s.StudentNumber, s.FirstName, s.MiddleName, s.LastName,
           s.CellPhone, s.Email, s.ImagePath, s.StreetAddress, s.Barangay,
           s.Municipality, s.City, s.GuardianId, s.RFIDCode, s.IsActive,
           s.CreatedDate, s.ModifiedDate,
           g.FirstName AS GuardianFirstName, g.LastName AS GuardianLastName,
           g.CellPhone AS GuardianCellPhone, g.Email AS GuardianEmail
    FROM Students s
    LEFT JOIN Guardians g ON s.GuardianId = g.GuardianId
    WHERE s.StudentId = @StudentId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetStudentByRFID
    @RFIDCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT s.StudentId, s.StudentNumber, s.FirstName, s.MiddleName, s.LastName,
           s.CellPhone, s.Email, s.ImagePath, s.StreetAddress, s.Barangay,
           s.Municipality, s.City, s.GuardianId, s.RFIDCode, s.IsActive,
           s.CreatedDate, s.ModifiedDate,
           g.FirstName AS GuardianFirstName, g.LastName AS GuardianLastName,
           g.CellPhone AS GuardianCellPhone, g.Email AS GuardianEmail
    FROM Students s
    LEFT JOIN Guardians g ON s.GuardianId = g.GuardianId
    WHERE s.RFIDCode = @RFIDCode AND s.IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_CreateStudent
    @StudentNumber NVARCHAR(50),
    @FirstName NVARCHAR(100),
    @MiddleName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @CellPhone NVARCHAR(20),
    @Email NVARCHAR(200),
    @ImagePath NVARCHAR(500) = NULL,
    @StreetAddress NVARCHAR(200),
    @Barangay NVARCHAR(100),
    @Municipality NVARCHAR(100),
    @City NVARCHAR(100),
    @GuardianId INT = NULL,
    @RFIDCode NVARCHAR(50) = NULL,
    @IsActive BIT,
    @StudentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO Students (StudentNumber, FirstName, MiddleName, LastName, CellPhone, Email,
                            ImagePath, StreetAddress, Barangay, Municipality, City, GuardianId, RFIDCode, IsActive)
        VALUES (@StudentNumber, @FirstName, @MiddleName, @LastName, @CellPhone, @Email,
                @ImagePath, @StreetAddress, @Barangay, @Municipality, @City, @GuardianId, @RFIDCode, @IsActive);
        
        SET @StudentId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateStudent
    @StudentNumber NVARCHAR(50) = NULL,
    @FirstName NVARCHAR(100) = NULL,
    @MiddleName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @CellPhone NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @ImagePath NVARCHAR(500) = NULL,
    @StreetAddress NVARCHAR(200) = NULL,
    @Barangay NVARCHAR(100) = NULL,
    @Municipality NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @GuardianId INT = NULL,
    @RFIDCode NVARCHAR(50) = NULL,
    @IsActive BIT = NULL,
    @StudentId INT,
    @Success BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if student exists
    IF NOT EXISTS(SELECT 1 FROM Students WHERE StudentId = @StudentId)
    BEGIN
        SET @Success = 0;
        RETURN;
    END
    
    BEGIN TRY
        -- Get current values to compare
        DECLARE @CurrentStudentNumber NVARCHAR(50), @CurrentFirstName NVARCHAR(100), @CurrentMiddleName NVARCHAR(100),
                @CurrentLastName NVARCHAR(100), @CurrentCellPhone NVARCHAR(20), @CurrentEmail NVARCHAR(200),
                @CurrentImagePath NVARCHAR(500), @CurrentStreetAddress NVARCHAR(200), @CurrentBarangay NVARCHAR(100),
                @CurrentMunicipality NVARCHAR(100), @CurrentCity NVARCHAR(100), @CurrentGuardianId INT,
                @CurrentRFIDCode NVARCHAR(50), @CurrentIsActive BIT;
        
        SELECT @CurrentStudentNumber = StudentNumber, @CurrentFirstName = FirstName, @CurrentMiddleName = MiddleName,
               @CurrentLastName = LastName, @CurrentCellPhone = CellPhone, @CurrentEmail = Email,
               @CurrentImagePath = ImagePath, @CurrentStreetAddress = StreetAddress, @CurrentBarangay = Barangay,
               @CurrentMunicipality = Municipality, @CurrentCity = City, @CurrentGuardianId = GuardianId,
               @CurrentRFIDCode = RFIDCode, @CurrentIsActive = IsActive
        FROM Students
        WHERE StudentId = @StudentId;
        
        -- Check if any values have actually changed
        DECLARE @HasChanges BIT = 0;
        
        IF (@StudentNumber IS NOT NULL AND (ISNULL(@CurrentStudentNumber, '') != ISNULL(@StudentNumber, ''))) OR
           (@FirstName IS NOT NULL AND (ISNULL(@CurrentFirstName, '') != ISNULL(@FirstName, ''))) OR
           (@MiddleName IS NOT NULL AND (ISNULL(@CurrentMiddleName, '') != ISNULL(@MiddleName, ''))) OR
           (@LastName IS NOT NULL AND (ISNULL(@CurrentLastName, '') != ISNULL(@LastName, ''))) OR
           (@CellPhone IS NOT NULL AND (ISNULL(@CurrentCellPhone, '') != ISNULL(@CellPhone, ''))) OR
           (@Email IS NOT NULL AND (ISNULL(@CurrentEmail, '') != ISNULL(@Email, ''))) OR
           (@ImagePath IS NOT NULL AND (ISNULL(@CurrentImagePath, '') != ISNULL(@ImagePath, ''))) OR
           (@StreetAddress IS NOT NULL AND (ISNULL(@CurrentStreetAddress, '') != ISNULL(@StreetAddress, ''))) OR
           (@Barangay IS NOT NULL AND (ISNULL(@CurrentBarangay, '') != ISNULL(@Barangay, ''))) OR
           (@Municipality IS NOT NULL AND (ISNULL(@CurrentMunicipality, '') != ISNULL(@Municipality, ''))) OR
           (@City IS NOT NULL AND (ISNULL(@CurrentCity, '') != ISNULL(@City, ''))) OR
           (@GuardianId IS NOT NULL AND (ISNULL(@CurrentGuardianId, 0) != ISNULL(@GuardianId, 0))) OR
           (@RFIDCode IS NOT NULL AND (ISNULL(@CurrentRFIDCode, '') != ISNULL(@RFIDCode, ''))) OR
           (@IsActive IS NOT NULL AND (ISNULL(@CurrentIsActive, 0) != ISNULL(@IsActive, 0)))
        BEGIN
            SET @HasChanges = 1;
        END
        
        -- Only proceed with update if there are actual changes
        IF @HasChanges = 1
        BEGIN
            UPDATE Students 
            SET StudentNumber = ISNULL(@StudentNumber, StudentNumber),
                FirstName = ISNULL(@FirstName, FirstName),
                MiddleName = ISNULL(@MiddleName, MiddleName),
                LastName = ISNULL(@LastName, LastName),
                CellPhone = ISNULL(@CellPhone, CellPhone),
                Email = ISNULL(@Email, Email),
                ImagePath = ISNULL(@ImagePath, ImagePath),
                StreetAddress = ISNULL(@StreetAddress, StreetAddress),
                Barangay = ISNULL(@Barangay, Barangay),
                Municipality = ISNULL(@Municipality, Municipality),
                City = ISNULL(@City, City),
                GuardianId = CASE WHEN @GuardianId IS NOT NULL THEN @GuardianId ELSE GuardianId END,
                RFIDCode = ISNULL(@RFIDCode, RFIDCode),
                IsActive = CASE WHEN @IsActive IS NOT NULL THEN @IsActive ELSE IsActive END,
                ModifiedDate = GETDATE()
            WHERE StudentId = @StudentId;
            
            SET @Success = 1;
        END
        ELSE
        BEGIN
            SET @Success = 1;
        END
        
    END TRY
    BEGIN CATCH
        -- Set failure flag with error details
        SET @Success = 0;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_GetStudentsByGuardianId
    @GuardianId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT s.StudentId, s.StudentNumber, s.FirstName, s.MiddleName, s.LastName,
           s.CellPhone, s.Email, s.ImagePath, s.StreetAddress, s.Barangay,
           s.Municipality, s.City, s.GuardianId, s.RFIDCode, s.IsActive,
           s.CreatedDate, s.ModifiedDate,
           g.FirstName AS GuardianFirstName, g.LastName AS GuardianLastName,
           g.CellPhone AS GuardianCellPhone, g.Email AS GuardianEmail
    FROM Students s
    LEFT JOIN Guardians g ON s.GuardianId = g.GuardianId
    WHERE g.GuardianId = @GuardianId;
END
GO
-----------------------------------------
-- Guardian Management Stored Procedures
-----------------------------------------
CREATE OR ALTER PROCEDURE sp_GetGuardianById
    @GuardianId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT GuardianId, FirstName, LastName, CellPhone, Email, IsActive, CreatedDate, ModifiedDate
    FROM Guardians
    WHERE IsActive = 1 AND GuardianId = @GuardianId
    ORDER BY LastName, FirstName;

END
GO

CREATE OR ALTER PROCEDURE sp_GetAllGuardians
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT GuardianId, FirstName, LastName, CellPhone, Email, IsActive, CreatedDate, ModifiedDate
    FROM Guardians
    WHERE IsActive = 1
    ORDER BY LastName, FirstName;
END
GO

CREATE OR ALTER PROCEDURE sp_CreateGuardian
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @CellPhone NVARCHAR(20),
    @Email NVARCHAR(200),
    @IsActive BIT,
    @GuardianId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO Guardians (FirstName, LastName, CellPhone, Email, IsActive)
        VALUES (@FirstName, @LastName, @CellPhone, @Email, @IsActive);
        
        SET @GuardianId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateGuardian
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @CellPhone NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @IsActive BIT = NULL,
    @GuardianId INT,
    @Success BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if student exists
    IF NOT EXISTS(SELECT 1 FROM Guardians WHERE GuardianId = @GuardianId)
    BEGIN
        SET @Success = 0;
        RETURN;
    END
    
    BEGIN TRY
        -- Get current values to compare
        DECLARE @CurrentFirstName NVARCHAR(100), @CurrentLastName NVARCHAR(100),
                @CurrentCellPhone NVARCHAR(20), @CurrentEmail NVARCHAR(200),
                @CurrentIsActive BIT;

        SELECT @CurrentFirstName = FirstName, @CurrentLastName = LastName
              ,@CurrentCellPhone = CellPhone, @CurrentEmail = Email
              ,@CurrentIsActive = IsActive
        FROM Guardians
        WHERE GuardianId = @GuardianId

        DECLARE @HasChanges BIT = 0;
        
        IF (@FirstName IS NOT NULL AND (ISNULL(@CurrentFirstName, '') != ISNULL(@FirstName, ''))) OR
           (@LastName IS NOT NULL AND (ISNULL(@CurrentLastName, '') != ISNULL(@LastName, ''))) OR
           (@CellPhone IS NOT NULL AND (ISNULL(@CurrentCellPhone, '') != ISNULL(@CellPhone, ''))) OR
           (@Email IS NOT NULL AND (ISNULL(@CurrentEmail, '') != ISNULL(@Email, ''))) OR
           (@IsActive IS NOT NULL AND (ISNULL(@CurrentIsActive, 0) != ISNULL(@IsActive, 0)))
        BEGIN
            SET @HasChanges = 1;
        END
        IF @HasChanges = 1
        BEGIN
            UPDATE Guardians 
            SET FirstName = ISNULL(@FirstName, FirstName),
                LastName = ISNULL(@LastName, LastName),
                CellPhone = ISNULL(@CellPhone, CellPhone),
                Email = ISNULL(@Email, Email),
                IsActive = CASE WHEN @IsActive IS NOT NULL THEN @IsActive ELSE IsActive END,
                ModifiedDate = GETDATE()
            WHERE GuardianId = @GuardianId AND IsActive = 1;
            
            SET @Success = 1;
        END
        ELSE
        BEGIN
            SET @Success = 1;
        END
    END TRY
    BEGIN CATCH
        THROW;
        SET @Success = 0;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_DeleteGuardian
    @GuardianId INT
   ,@Success BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
-- Check if student exists
    IF NOT EXISTS(SELECT 1 FROM Guardians WHERE GuardianId = @GuardianId)
    BEGIN
        SET @Success = 0;
        RETURN;
    END

    BEGIN TRY
        UPDATE Guardians
        SET IsActive = 0
        WHERE GuardianId = @GuardianId AND IsActive = 1
         SET @Success = 1;
    END TRY
    BEGIN CATCH
        THROW;
        SET @Success = 0;
    END CATCH
END
GO
-------------------------------------------
-- Attendance Management Stored Procedures
-------------------------------------------
CREATE OR ALTER   PROCEDURE [dbo].[sp_RecordAttendance]
    @StudentId INT,
    @Type INT, -- 1=TimeIn, 2=TimeOut
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TimeValue DATETIME2 = GETDATE();
    
    BEGIN TRY
        IF @Type = 0 -- Time In
        BEGIN
            INSERT INTO AttendanceRecords (StudentId, TimeIn, Type, Notes)
            VALUES (@StudentId, @TimeValue, @Type, @Notes);
        END
        ELSE IF @Type = 1 -- Time Out
        BEGIN
            UPDATE AttendanceRecords 
            SET [TimeOut] = @TimeValue, [Type] = @Type, Notes = @Notes
            WHERE StudentId = @StudentId AND [Type] = 0 --Time In
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_GetStudentAttendanceToday
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT AttendanceId, StudentId, TimeIn, TimeOut, Type, Notes, RecordedDate
    FROM AttendanceRecords
    WHERE StudentId = @StudentId 
    AND CAST(RecordedDate AS DATE) = CAST(GETDATE() AS DATE)
    ORDER BY RecordedDate DESC;
END
GO

-- SMS Management Stored Procedures
CREATE OR ALTER PROCEDURE sp_LogSMS
    @StudentId INT,
    @PhoneNumber NVARCHAR(20),
    @Message NVARCHAR(1000),
    @Status INT = 1, -- Default to Pending
    @ErrorMessage NVARCHAR(1000) = NULL,
    @ProviderResponse NVARCHAR(2000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO SMSLogs (StudentId, PhoneNumber, Message, Status, ErrorMessage, ProviderResponse)
    VALUES (@StudentId, @PhoneNumber, @Message, @Status, @ErrorMessage, @ProviderResponse);
END
GO

CREATE OR ALTER PROCEDURE sp_GetSMSConfiguration
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 1 ConfigId, ProviderName, ApiKey, ApiUrl, SenderName, IsActive, CreatedDate, ModifiedDate
    FROM SMSConfiguration
    WHERE IsActive = 1
    ORDER BY CreatedDate DESC;
END
GO

-----------------------------------
-- RFID Management Functions
-----------------------------------
CREATE OR ALTER PROCEDURE sp_GenerateRFIDCode
    @RFIDCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MaxAttempts INT = 100; -- Safety net to prevent infinite loops
    DECLARE @Attempts INT = 0;
    
    WHILE @Attempts < @MaxAttempts
    BEGIN
        SET @RFIDCode = 'RFID' + FORMAT(ABS(CHECKSUM(NEWID())) % 1000000, '000000');
        
        IF NOT EXISTS (SELECT 1 FROM Students WITH (NOLOCK) WHERE RFIDCode = @RFIDCode)
            BREAK;
            
        SET @Attempts = @Attempts + 1;
    END
    
    -- If we exhausted max attempts, throw an error
    IF @Attempts >= @MaxAttempts
    BEGIN
        RAISERROR('Unable to generate unique RFID code after %d attempts', 16, 1, @MaxAttempts);
        RETURN;
    END
END
GO

CREATE OR ALTER PROCEDURE sp_AssignRFIDToStudent
    @StudentId INT,
    @RFIDCode NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- If no RFID code provided, generate one
        IF @RFIDCode IS NULL
        BEGIN
            EXEC sp_GenerateRFIDCode @RFIDCode = @RFIDCode OUTPUT;
        END
        
        -- Check if student already has an RFID
        IF EXISTS (SELECT 1 FROM Students WHERE StudentId = @StudentId AND RFIDCode IS NOT NULL)
        BEGIN
            RAISERROR('Student already has an RFID assigned.', 16, 1);
            RETURN;
        END
        
        -- Check if RFID code is already in use
        IF EXISTS (SELECT 1 FROM Students WHERE RFIDCode = @RFIDCode)
        BEGIN
            RAISERROR('RFID code is already assigned to another student.', 16, 1);
            RETURN;
        END
        
        -- Check if student exists
        IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentId = @StudentId)
        BEGIN
            RAISERROR('Student with ID %d does not exist.', 16, 1, @StudentId);
            RETURN;
        END
        
        UPDATE Students
        SET RFIDCode = @RFIDCode,
            ModifiedDate = GETDATE()
        WHERE StudentId = @StudentId;
        
        -- Check if update was successful
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Failed to assign RFID code to student.', 16, 1);
            RETURN;
        END
        
        SELECT @RFIDCode AS AssignedRFIDCode;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
---------08-31-2025
CREATE OR ALTER PROCEDURE [dbo].[sp_GetStudentAttendanceStatus]
    @StudentId INT,
    @CurrentStatus NVARCHAR(10) OUTPUT,
    @LastTimeStamp DATETIME OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LastAttendanceType INT;
    
    -- Get the most recent attendance record for today
    SELECT TOP 1 
        @LastAttendanceType = [Type],
        @LastTimeStamp = RecordedDate
    FROM [dbo].[AttendanceRecords]
    WHERE StudentId = @StudentId
        AND CAST(RecordedDate AS DATE) = CAST(GETDATE() AS DATE)
    ORDER BY RecordedDate DESC;
    
    -- Determine current status based on AttendanceType enum
    -- Assuming: TimeIn = 0, TimeOut = 1 (adjust based on your enum values)
    IF @LastAttendanceType IS NULL
        SET @CurrentStatus = 'IN'; -- Never clocked in today
    IF @LastAttendanceType = 0 -- TimeIn
        SET @CurrentStatus = 'OUT';  -- Last action was clock in
    ELSE IF @LastAttendanceType = 1 -- TimeOut
        SET @CurrentStatus = 'IN'; -- Last action was clock out
    ELSE
        SET @CurrentStatus = 'OUT'; -- Default to out for safety
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ValidateAttendanceAction]
    @StudentId INT,
    @ProposedAttendanceType NVARCHAR(10),
    @IsValid BIT OUTPUT,
    @ValidationMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LastTimeStamp DATETIME;
    DECLARE @MinutesSinceLastScan INT;
    
    -- Get last attendance record timestamp
    SELECT TOP 1 
        @LastTimeStamp = RecordedDate
    FROM [dbo].[AttendanceRecords]
    WHERE StudentId = @StudentId
        AND CAST(RecordedDate AS DATE) = CAST(GETDATE() AS DATE)
    ORDER BY RecordedDate DESC;
    
    -- Calculate minutes since last scan
    IF @LastTimeStamp IS NOT NULL
        SET @MinutesSinceLastScan = DATEDIFF(MINUTE, @LastTimeStamp, GETDATE());
    ELSE
        SET @MinutesSinceLastScan = 999; -- No previous record
    
    -- Validation Rule: Prevent rapid successive scans (within 5 minutes)
    IF @MinutesSinceLastScan < 5
    BEGIN
        SET @IsValid = 0;
        SET @ValidationMessage = 'Please wait at least 5 minutes between consecutive scans.';
        RETURN;
    END
    
    -- Additional business rules can be added here
    
    SET @IsValid = 1;
    SET @ValidationMessage = 'Attendance action is valid.';
END
GO