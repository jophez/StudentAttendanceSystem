USE StudentAttendanceDB;
GO

-------------------------------------
-- User Management Stored Procedures
-------------------------------------
/****** Object:  StoredProcedure [dbo].[sp_AssignRFIDToStudent]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_AssignRFIDToStudent]
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
/****** Object:  StoredProcedure [dbo].[sp_CreateGuardian]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_CreateGuardian]
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
/****** Object:  StoredProcedure [dbo].[sp_CreateStudent]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_CreateStudent]
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
/****** Object:  StoredProcedure [dbo].[sp_CreateUser]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_CreateUser]
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
/****** Object:  StoredProcedure [dbo].[sp_DeleteGuardian]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   PROCEDURE [dbo].[sp_DeleteGuardian]
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
/****** Object:  StoredProcedure [dbo].[sp_DeleteStudent]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   PROCEDURE [dbo].[sp_DeleteStudent]
    @StudentId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT * FROM STUDENTS s WHERE s.StudentId = @StudentId AND s.IsActive = 1
        BEGIN
            UPDATE STUDENTS
            SET IsActive = 0
            WHERE  StudentId = @StudentId AND IsActive = 1
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_DeleteUser]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_DeleteUser]
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
/****** Object:  StoredProcedure [dbo].[sp_GenerateRFIDCode]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- RFID Management Functions
CREATE OR ALTER   PROCEDURE [dbo].[sp_GenerateRFIDCode]
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
/****** Object:  StoredProcedure [dbo].[sp_GetAllGuardians]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Guardian Management Stored Procedures
CREATE OR ALTER   PROCEDURE [dbo].[sp_GetAllGuardians]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT GuardianId, FirstName, LastName, CellPhone, Email, IsActive, CreatedDate, ModifiedDate
    FROM Guardians
    WHERE IsActive = 1
    ORDER BY LastName, FirstName;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetAllStudents]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Student Management Stored Procedures
CREATE OR ALTER   PROCEDURE [dbo].[sp_GetAllStudents]
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
/****** Object:  StoredProcedure [dbo].[sp_GetAllUsers]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_GetAllUsers]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Username, FirstName, LastName, Email, ImagePath, Role, IsActive, CreatedDate, ModifiedDate
    FROM Users
    ORDER BY LastName, FirstName;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetGuardianById]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   PROCEDURE [dbo].[sp_GetGuardianById]
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
/****** Object:  StoredProcedure [dbo].[sp_GetSMSConfiguration]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_GetSMSConfiguration]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 1 ConfigId, ProviderName, ApiKey, ApiUrl, SenderName, IsActive, CreatedDate, ModifiedDate
    FROM SMSConfiguration
    WHERE IsActive = 1
    ORDER BY CreatedDate DESC;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetSMSLogs]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get SMS Logs with Filtering
CREATE OR ALTER   PROCEDURE [dbo].[sp_GetSMSLogs]
    @StudentId INT = NULL,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @MaxRecords INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxRecords)
        s.LogId,
        s.StudentId,
        s.PhoneNumber,
        s.Message,
        s.Status,
        s.ErrorMessage,
        s.SentDate,
        s.ProviderResponse,
        st.FirstName + ' ' + st.LastName AS StudentName,
        st.StudentNumber
    FROM SMSLogs s
    LEFT JOIN Students st ON s.StudentId = st.StudentId
    WHERE (@StudentId IS NULL OR s.StudentId = @StudentId)
    AND (@FromDate IS NULL OR s.SentDate >= @FromDate)
    AND (@ToDate IS NULL OR s.SentDate <= @ToDate)
    ORDER BY s.SentDate DESC;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetSMSStatistics]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get SMS Statistics
CREATE OR ALTER   PROCEDURE [dbo].[sp_GetSMSStatistics]
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FromDate IS NULL SET @FromDate = CAST(GETDATE() AS DATE);
    IF @ToDate IS NULL SET @ToDate = DATEADD(DAY, 1, CAST(GETDATE() AS DATE));
    
    SELECT 
        COUNT(*) AS TotalSent,
        COUNT(CASE WHEN Status = 1 THEN 1 END) AS Pending,
        COUNT(CASE WHEN Status = 2 THEN 1 END) AS Sent,
        COUNT(CASE WHEN Status = 3 THEN 1 END) AS Failed,
        COUNT(CASE WHEN Status = 4 THEN 1 END) AS Delivered,
        COUNT(CASE WHEN Status = 5 THEN 1 END) AS Queued
    FROM SMSLogs
    WHERE SentDate >= @FromDate AND SentDate < @ToDate;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetStudentAttendanceStatus]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_GetStudentAttendanceStatus]
    @StudentId INT,
    @CurrentStatus NVARCHAR(10) OUTPUT,
    @LastTimeStamp DATETIME OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TimeOut DATETIME;
    
    -- Get the most recent attendance record for today
    SELECT TOP 1 
        @TimeOut = [TimeOut],
        @LastTimeStamp = COALESCE([TimeOut], [TimeIn]) -- Last action timestamp
    FROM [dbo].[AttendanceRecords]
    WHERE StudentId = @StudentId
        AND CAST([TimeIn] AS DATE) = CAST(GETDATE() AS DATE)
    ORDER BY [TimeIn] DESC;
    
    -- Determine current status based on whether TimeOut is set
    IF @TimeOut IS NULL AND @LastTimeStamp IS NOT NULL
        SET @CurrentStatus = 'IN';  -- Has TimeIn but no TimeOut = currently IN
    ELSE
        SET @CurrentStatus = 'OUT'; -- Either no record today or has TimeOut = currently OUT
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetStudentAttendanceToday]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_GetStudentAttendanceToday]
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
/****** Object:  StoredProcedure [dbo].[sp_GetStudentById]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_GetStudentById]
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT s.StudentId, s.StudentNumber, s.FirstName, s.MiddleName, s.LastName,
           s.CellPhone, s.Email, s.ImagePath, s.StreetAddress, s.Barangay,
           s.Municipality, s.City, s.GuardianId, s.RFIDCode, s.IsActive,
           s.CreatedDate, s.ModifiedDate,
           g.FirstName AS GuardianFirstName, g.LastName AS GuardianLastName,
           g.CellPhone AS GuardianCellPhone, g.Email AS GuardianEmail,
           atr.TimeIn AS TimeIn, atr.[TimeOut] AS TimeOut, atr.[Type] AS Type
    FROM Students s
    LEFT JOIN Guardians g ON s.GuardianId = g.GuardianId
    LEFT JOIN AttendanceRecords atr ON atr.StudentId = s.StudentId
    WHERE s.StudentId = @StudentId
    ORDER BY atr.TimeIn DESC
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetStudentByRFID]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_GetStudentByRFID]
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
/****** Object:  StoredProcedure [dbo].[sp_GetStudentsByGuardianId]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   PROCEDURE [dbo].[sp_GetStudentsByGuardianId]
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
/****** Object:  StoredProcedure [dbo].[sp_GetUserById]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   PROCEDURE [dbo].[sp_GetUserById]
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
/****** Object:  StoredProcedure [dbo].[sp_LogSMS]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Enhanced SMS Logging
CREATE OR ALTER   PROCEDURE [dbo].[sp_LogSMS]
    @StudentId INT = NULL,
    @PhoneNumber NVARCHAR(20),
    @Message NVARCHAR(1000),
    @Status INT = 1, -- Default to Pending
    @ErrorMessage NVARCHAR(1000) = NULL,
    @ProviderResponse NVARCHAR(2000) = NULL,
    @LogId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO SMSLogs (StudentId, PhoneNumber, Message, Status, ErrorMessage, ProviderResponse, SentDate)
        VALUES (@StudentId, @PhoneNumber, @Message, @Status, @ErrorMessage, @ProviderResponse, GETDATE());
        
        SET @LogId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[sp_RecordAttendance]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_RecordAttendance]
    @StudentId INT,
    @Type INT, -- 1=TimeIn, 0=TimeOut
    @Notes NVARCHAR(500) = NULL,
    @MinimumMinutes INT -- Default minimum duration (adjust as needed)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TimeValue DATETIME2 = GETDATE();
    DECLARE @LastTimeIn DATETIME2;
    DECLARE @MinutesSinceTimeIn INT;
    
    BEGIN TRY
        IF @Type = 1 -- Time In
        BEGIN
            -- Check if already timed in today without timing out
            IF EXISTS (
                SELECT 1 
                FROM AttendanceRecords 
                WHERE StudentId = @StudentId 
                  AND TimeOut IS NULL
                  AND CAST(TimeIn AS DATE) = CAST(@TimeValue AS DATE)
            )
            BEGIN
                RAISERROR('Student is already timed in. Please time out first.', 16, 1);
                RETURN;
            END
            
            -- Insert new Time In record
            INSERT INTO AttendanceRecords (StudentId, TimeIn, Type, Notes, RecordedDate)
            VALUES (@StudentId, @TimeValue, @Type, @Notes, @TimeValue);
        END
        ELSE IF @Type = 0 -- Time Out
        BEGIN
            -- Get the most recent TimeIn record without a TimeOut for today
            SELECT TOP 1 
                @LastTimeIn = TimeIn
            FROM AttendanceRecords
            WHERE StudentId = @StudentId 
              AND TimeOut IS NULL
              AND CAST(TimeIn AS DATE) = CAST(@TimeValue AS DATE)
            ORDER BY TimeIn DESC;
            
            -- Check if student has timed in
            IF @LastTimeIn IS NULL
            BEGIN
                RAISERROR('Student must time in before timing out.', 16, 1);
                RETURN;
            END
            
            -- Calculate minutes since time in
            SET @MinutesSinceTimeIn = DATEDIFF(MINUTE, @LastTimeIn, @TimeValue);
            
            -- Check if minimum duration has been met
            IF @MinutesSinceTimeIn < @MinimumMinutes
            BEGIN
                DECLARE @ErrorMsg NVARCHAR(200) = 
                    'Student must be timed in for at least ' + 
                    CAST(@MinimumMinutes AS NVARCHAR(10)) + 
                    ' minutes. Current duration: ' + 
                    CAST(@MinutesSinceTimeIn AS NVARCHAR(10)) + ' minutes.';
                RAISERROR(@ErrorMsg, 16, 1);
                RETURN;
            END
            
            -- Update the TimeIn record with TimeOut
            UPDATE AttendanceRecords
            SET [TimeOut] = @TimeValue
            WHERE StudentId = @StudentId 
              AND TimeOut IS NULL
              AND TimeIn = @LastTimeIn;
        END
        ELSE
        BEGIN
            RAISERROR('Invalid attendance type. Use 1 for Time In or 0 for Time Out.', 16, 1);
            RETURN;
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_SaveSMSConfiguration]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- SMS Configuration Management
CREATE OR ALTER   PROCEDURE [dbo].[sp_SaveSMSConfiguration]
    @ConfigId INT = NULL,
    @ProviderName NVARCHAR(100),
    @ApiKey NVARCHAR(500),
    @ApiUrl NVARCHAR(500),
    @SenderName NVARCHAR(100),
    @IsActive BIT,
    @OutputConfigId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Deactivate all existing configurations
        UPDATE SMSConfiguration SET IsActive = 0;
        
        IF @ConfigId IS NULL OR @ConfigId = 0
        BEGIN
            -- Insert new configuration
            INSERT INTO SMSConfiguration (ProviderName, ApiKey, ApiUrl, SenderName, IsActive, CreatedDate)
            VALUES (@ProviderName, @ApiKey, @ApiUrl, @SenderName, @IsActive, GETDATE());
            
            SET @OutputConfigId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- Update existing configuration
            UPDATE SMSConfiguration
            SET ProviderName = @ProviderName,
                ApiKey = @ApiKey,
                ApiUrl = @ApiUrl,
                SenderName = @SenderName,
                IsActive = @IsActive,
                ModifiedDate = GETDATE()
            WHERE ConfigId = @ConfigId;
            
            SET @OutputConfigId = @ConfigId;
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[sp_SendAttendanceSMS]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Send Attendance SMS Template
CREATE OR ALTER   PROCEDURE [dbo].[sp_SendAttendanceSMS]
    @StudentId INT,
    @AttendanceType INT, -- 1=TimeIn, 2=TimeOut
    @ScanTime DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @GuardianPhone NVARCHAR(20);
    DECLARE @StudentName NVARCHAR(300);
    DECLARE @Message NVARCHAR(1000);
    DECLARE @TypeText NVARCHAR(20);
    DECLARE @LogId INT;
    
    -- Get student and guardian information
    SELECT 
        @GuardianPhone = g.CellPhone,
        @StudentName = s.FirstName + ' ' + s.LastName
    FROM Students s
    INNER JOIN Guardians g ON s.GuardianId = g.GuardianId
    WHERE s.StudentId = @StudentId AND s.IsActive = 1 AND g.IsActive = 1;
    
    IF @GuardianPhone IS NOT NULL
    BEGIN
        -- Determine message type
        SET @TypeText = CASE WHEN @AttendanceType = 1 THEN 'arrived at' ELSE 'left' END;
        
        -- CREATE OR ALTER message
        SET @Message = 'Your child ' + @StudentName + ' has ' + @TypeText + ' school at ' + 
                      FORMAT(@ScanTime, 'HH:mm') + ' on ' + FORMAT(@ScanTime, 'yyyy-MM-dd') + 
                      '. - School Attendance System';
        
        -- Log the SMS (status will be updated by the SMS service)
        EXEC sp_LogSMS 
            @StudentId = @StudentId,
            @PhoneNumber = @GuardianPhone,
            @Message = @Message,
            @Status = 1, -- Pending
            @LogId = @LogId OUTPUT;
            
        -- Return the message details for the SMS service to process
        SELECT 
            @LogId AS LogId,
            @GuardianPhone AS PhoneNumber,
            @Message AS Message,
            @StudentName AS StudentName;
    END
    ELSE
    BEGIN
        -- No guardian phone number available
        RAISERROR('No guardian phone number found for student ID %d', 16, 1, @StudentId);
    END
END

GO
/****** Object:  StoredProcedure [dbo].[sp_SendBulkAnnouncementSMS]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Bulk SMS for Announcements
CREATE OR ALTER   PROCEDURE [dbo].[sp_SendBulkAnnouncementSMS]
    @Message NVARCHAR(1000),
    @SendToAllGuardians BIT = 1,
    @StudentIds NVARCHAR(MAX) = NULL -- Comma-separated list of student IDs
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Recipients TABLE (
        StudentId INT,
        GuardianPhone NVARCHAR(20),
        StudentName NVARCHAR(300)
    );
    
    IF @SendToAllGuardians = 1
    BEGIN
        -- Send to all active guardians
        INSERT INTO @Recipients
        SELECT 
            s.StudentId,
            g.CellPhone,
            s.FirstName + ' ' + s.LastName
        FROM Students s
        INNER JOIN Guardians g ON s.GuardianId = g.GuardianId
        WHERE s.IsActive = 1 AND g.IsActive = 1
        AND g.CellPhone IS NOT NULL AND g.CellPhone <> '';
    END
    ELSE IF @StudentIds IS NOT NULL
    BEGIN
        -- Send to specific students' guardians
        INSERT INTO @Recipients
        SELECT 
            s.StudentId,
            g.CellPhone,
            s.FirstName + ' ' + s.LastName
        FROM Students s
        INNER JOIN Guardians g ON s.GuardianId = g.GuardianId
        WHERE s.IsActive = 1 AND g.IsActive = 1
        AND g.CellPhone IS NOT NULL AND g.CellPhone <> ''
        AND s.StudentId IN (SELECT value FROM STRING_SPLIT(@StudentIds, ','));
    END
    
    -- Log SMS for each recipient
    DECLARE @StudentId INT, @Phone NVARCHAR(20), @Name NVARCHAR(300);
    DECLARE recipient_cursor CURSOR FOR 
    SELECT StudentId, GuardianPhone, StudentName FROM @Recipients;
    DECLARE @LogId INT;

    OPEN recipient_cursor;
    FETCH NEXT FROM recipient_cursor INTO @StudentId, @Phone, @Name;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- CREATE OR ALTER personalized message
        DECLARE @PersonalizedMessage NVARCHAR(1000);
        SET @PersonalizedMessage = 'Dear Parent/Guardian of ' + @Name + ', ' + @Message + 
                                  ' - School Attendance System';
        
        -- Log the SMS
        EXEC sp_LogSMS 
            @StudentId = @StudentId,
            @PhoneNumber = @Phone,
            @Message = @PersonalizedMessage,
            @Status = 1, -- Pending
            @LogId = @LogId OUTPUT;
        
        FETCH NEXT FROM recipient_cursor INTO @StudentId, @Phone, @Name;
    END
    
    CLOSE recipient_cursor;
    DEALLOCATE recipient_cursor;
    
    -- Return count of messages queued
    SELECT COUNT(*) AS MessagesQueued FROM @Recipients;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_sp_GetStudentsByGuardianId]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   PROCEDURE [dbo].[sp_sp_GetStudentsByGuardianId]
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
/****** Object:  StoredProcedure [dbo].[sp_UpdateGuardian]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   PROCEDURE [dbo].[sp_UpdateGuardian]
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
            WHERE GuardianId = @GuardianId;
            
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
/****** Object:  StoredProcedure [dbo].[sp_UpdateSMSLog]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Update SMS Log Status
CREATE OR ALTER   PROCEDURE [dbo].[sp_UpdateSMSLog]
    @LogId INT,
    @Status INT,
    @ErrorMessage NVARCHAR(1000) = NULL,
    @ProviderResponse NVARCHAR(2000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE SMSLogs
    SET Status = @Status,
        ErrorMessage = @ErrorMessage,
        ProviderResponse = @ProviderResponse
    WHERE LogId = @LogId;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateStudent]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   PROCEDURE [dbo].[sp_UpdateStudent]
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
/****** Object:  StoredProcedure [dbo].[sp_UpdateUser]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_UpdateUser]
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
/****** Object:  StoredProcedure [dbo].[sp_ValidateAttendanceAction]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_ValidateAttendanceAction]
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
/****** Object:  StoredProcedure [dbo].[sp_ValidateUser]    Script Date: 1/17/2026 10:53:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- User Management Stored Procedures
CREATE OR ALTER   PROCEDURE [dbo].[sp_ValidateUser]
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
