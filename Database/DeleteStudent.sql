/****** Object:  StoredProcedure [dbo].[sp_DeleteStudent]    Script Date: 1/16/2026 10:23:25 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_DeleteStudent]
    @StudentId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 1 FROM STUDENTS s WHERE s.StudentNumber = @StudentId AND s.IsActive = 1
        BEGIN
           UPDATE STUDENTS
           SET IsActive = 0
           WHERE  StudentNumber = @StudentId AND IsActive = 1
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END

GO