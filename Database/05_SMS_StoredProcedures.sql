USE StudentAttendanceDB;
GO

-- SMS Configuration Management
CREATE OR ALTER PROCEDURE sp_SaveSMSConfiguration
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

-- Enhanced SMS Logging
CREATE OR ALTER PROCEDURE sp_LogSMS
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

-- Update SMS Log Status
CREATE OR ALTER PROCEDURE sp_UpdateSMSLog
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

-- Get SMS Logs with Filtering
CREATE OR ALTER PROCEDURE sp_GetSMSLogs
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

-- Get SMS Statistics
CREATE OR ALTER PROCEDURE sp_GetSMSStatistics
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

-- Send Attendance SMS Template
CREATE OR ALTER PROCEDURE sp_SendAttendanceSMS
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
        
        -- Create message
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

-- Bulk SMS for Announcements
CREATE OR ALTER PROCEDURE sp_SendBulkAnnouncementSMS
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
    
    OPEN recipient_cursor;
    FETCH NEXT FROM recipient_cursor INTO @StudentId, @Phone, @Name;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Create personalized message
        DECLARE @PersonalizedMessage NVARCHAR(1000);
        SET @PersonalizedMessage = 'Dear Parent/Guardian of ' + @Name + ', ' + @Message + 
                                  ' - School Attendance System';
        
        -- Log the SMS
        EXEC sp_LogSMS 
            @StudentId = @StudentId,
            @PhoneNumber = @Phone,
            @Message = @PersonalizedMessage,
            @Status = 1; -- Pending
        
        FETCH NEXT FROM recipient_cursor INTO @StudentId, @Phone, @Name;
    END
    
    CLOSE recipient_cursor;
    DEALLOCATE recipient_cursor;
    
    -- Return count of messages queued
    SELECT COUNT(*) AS MessagesQueued FROM @Recipients;
END
GO