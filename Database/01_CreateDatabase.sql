-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'StudentAttendanceDB')
BEGIN
    CREATE DATABASE StudentAttendanceDB;
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.server_principals where type in ('U','S','G') AND [name] = 'StudentAttendanceUsr')
BEGIN
	CREATE LOGIN StudentAttendanceUsr
		WITH PASSWORD = 'StudentAttendanceUsr!'
END
GO

USE StudentAttendanceDB
GO

CREATE SCHEMA StudentAttendanceUsr
GO

CREATE USER StudentAttendanceUsr
	WITH DEFAULT_SCHEMA = StudentAttendanceUsr
GO