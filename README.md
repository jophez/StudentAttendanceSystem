# Student Attendance System

A comprehensive Windows Forms application for managing student attendance with RFID scanning capabilities and SMS notifications.

## Features

### Authentication & Authorization
- Modal login screen with user image display
- Role-based access control (Administrator vs Regular User)
- Secure password hashing using SHA-256

### Administrator Features
- **User Management**: Full CRUD operations for system users with role assignment
- **Student Management**: Complete student information management
- **Guardian Management**: Parent/guardian contact information management
- **RFID Management**: 
  - RFID reader configuration and calibration
  - RFID code generation (one per student)
- **SMS Provider Configuration**: Semaphore API integration setup

### Regular User Features
- **Student Information**: View and update student time in/out and contact information
- **Guardian Information**: View and update guardian contact information
- Limited access with administrator approval required for certain fields

### Main Dashboard
- Grid-based display showing all student information
- Double-click to view detailed student information modal
- Real-time clock display
- Current user and role information in status bar

### RFID Attendance System
- Time in/out tracking via RFID card scanning
- Automatic SMS notifications to guardians via Semaphore
- Student verification and attendance logging

### Windows Service
- External attendance display with running clock
- Real-time student information display during RFID scanning
- Automatic SMS notifications to parents/guardians

## Technical Stack

### Backend
- **Database**: SQL Server with optimized indexed tables
- **Data Access**: ADO.NET with stored procedures
- **Architecture**: Repository pattern with separation of concerns

### Frontend
- **Framework**: Windows Forms (.NET 8)
- **UI**: Professional grid-based interface with modal dialogs

### Projects Structure
- **StudentAttendanceSystem.Core**: Domain models and interfaces
- **StudentAttendanceSystem.Data**: Data access layer with repositories
- **StudentAttendanceSystem.WinForms**: Main Windows Forms application
- **StudentAttendanceSystem.Service**: Windows Service for external display

## Database Schema

### Main Tables
- **Users**: System users with role-based permissions
- **Students**: Complete student information with RFID codes
- **Guardians**: Parent/guardian contact information
- **AttendanceRecords**: Time in/out tracking
- **SMSLogs**: SMS notification history
- **SMSConfiguration**: Semaphore API settings
- **RFIDConfiguration**: RFID reader settings

### Key Features
- Indexed tables for optimized query performance
- Stored procedures for all CRUD operations
- Built-in functions for RFID code generation
- Referential integrity with foreign key constraints

## Setup Instructions

### Database Setup
1. Run the SQL scripts in order:
   - `Database/01_CreateDatabase.sql`
   - `Database/02_CreateTables.sql`
   - `Database/03_StoredProcedures.sql`
   - `Database/04_SampleData.sql`

### Application Configuration
1. Update connection string in `DatabaseConnection.cs` if needed
2. Configure Semaphore API credentials in SMS Configuration
3. Set up RFID reader port and settings

### Default Login Credentials
- **Administrator**: username: `admin`, password: `admin123`
- **Regular User**: username: `user1`, password: `user123`

### SMS Integration
- Configured for Semaphore API
- Automatic notifications sent to guardians on attendance events
- Template messages for time in/out events

## Security Features
- Password hashing with SHA-256
- Role-based access control
- Least privilege principle implementation
- Input validation and error handling throughout

## Validation & Error Handling
- Comprehensive input validation
- User-friendly error messages
- Success/warning notifications
- Database transaction handling

## Future Enhancements
- Real RFID hardware integration
- Advanced reporting features
- Mobile app companion
- Biometric authentication
- Cloud synchronization capabilities