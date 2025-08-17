# SMS Semaphore Integration Guide

## Overview
This Student Attendance System includes comprehensive SMS notification functionality using the Semaphore SMS API. The system automatically sends notifications to guardians when students scan their RFID cards for attendance.

## Semaphore SMS Provider Setup

### 1. Create Semaphore Account
1. **Visit**: [https://semaphore.co/](https://semaphore.co/)
2. **Sign Up** for a new account or **Log In** if you already have one
3. **Verify** your account via email
4. **Add Credits** to your account for sending SMS messages

### 2. Get API Credentials
1. **Dashboard**: Log in to your Semaphore dashboard
2. **API Key**: Navigate to Settings → API Keys
3. **Copy** your API Key (keep this secure)
4. **Sender Name**: Choose or register a sender name (max 11 characters)

### 3. Application Configuration
1. **Run** the Student Attendance System as Administrator
2. **Navigate** to Menu → SMS Provider → Configure SMS
3. **Enter** your Semaphore credentials:
   - **Provider Name**: Semaphore (pre-filled)
   - **API Key**: Your Semaphore API key
   - **API URL**: https://api.semaphore.co/api/v4/messages (pre-filled)
   - **Sender Name**: Your registered sender name
   - **Active**: Check this box to enable SMS

## SMS Configuration Form Features

### Provider Settings
- **Secure API Key Storage**: Password-masked input for security
- **Validation**: Real-time validation of required fields
- **Active Toggle**: Enable/disable SMS functionality

### Testing & Validation
- **Test Connection**: Verify API credentials and connectivity
- **Send Test SMS**: Send a test message to verify functionality
- **Check Balance**: View current account credit balance
- **Real-time Status**: Connection status with color indicators

### Message Templates
- **Time In Template**: Customizable message for student arrival
- **Time Out Template**: Customizable message for student departure
- **Template Variables**: {StudentName}, {Time}, {Date} placeholders
- **Preview Function**: See how messages will appear

### Statistics & Monitoring
- **Daily Count**: Number of SMS sent today
- **Balance Display**: Current account balance
- **Last Test**: Timestamp of last connection test
- **Connection Status**: Real-time connection monitoring

## SMS Notification System

### Automatic Attendance Notifications
When a student scans their RFID card:
1. **Student Verification**: System identifies student by RFID
2. **Guardian Lookup**: Retrieves guardian phone number
3. **Message Creation**: Generates personalized attendance message
4. **SMS Sending**: Sends via Semaphore API
5. **Database Logging**: Records SMS attempt with status
6. **Visual Feedback**: Displays success/failure on attendance screen

### Message Format Examples
```
Time In: "Your child Juan Dela Cruz has arrived at school at 07:30 on 2024-12-15. - School Attendance System"

Time Out: "Your child Juan Dela Cruz has left school at 16:00 on 2024-12-15. - School Attendance System"
```

### Phone Number Format
The system automatically formats Philippine phone numbers:
- **Input**: 09123456789, 9123456789, +639123456789
- **Output**: +639123456789 (standard international format)

## SMS Logs and Monitoring

### Comprehensive Logging
- **All SMS Tracked**: Every SMS attempt is logged
- **Status Tracking**: Pending, Sent, Failed, Delivered, Queued
- **Error Messages**: Detailed error information for failed sends
- **Timestamp**: Exact send time for each message
- **Student Association**: Links SMS to specific student records

### SMS Logs Viewer
Access via Menu → SMS Provider → Configure SMS → View SMS Logs

**Features**:
- **Date Range Filtering**: Filter by date range
- **Status Filtering**: Filter by SMS status
- **Export to CSV**: Export logs for analysis
- **Detailed View**: Double-click for full message details
- **Color Coding**: Visual status indicators
- **Search Functionality**: Find specific records

### Log Statuses
- 🟡 **Pending**: SMS queued for sending
- 🟢 **Sent**: Successfully sent to Semaphore
- 🔵 **Delivered**: Confirmed delivery to recipient
- 🔴 **Failed**: Failed to send (with error message)
- 🟠 **Queued**: In Semaphore queue

## Database Schema

### SMS Configuration Table
```sql
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
```

### SMS Logs Table
```sql
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
```

### Stored Procedures
- **sp_SaveSMSConfiguration**: Save/update SMS settings
- **sp_LogSMS**: Log SMS attempts
- **sp_UpdateSMSLog**: Update SMS status
- **sp_GetSMSLogs**: Retrieve filtered SMS logs
- **sp_SendAttendanceSMS**: Process attendance notifications
- **sp_SendBulkAnnouncementSMS**: Send bulk messages

## API Integration Details

### Semaphore API Endpoints
- **Send SMS**: POST https://api.semaphore.co/api/v4/messages
- **Account Info**: GET https://api.semaphore.co/api/v4/account
- **Message Status**: GET https://api.semaphore.co/api/v4/messages/{messageId}

### Request Format
```json
{
    "apikey": "your-api-key",
    "number": "+639123456789",
    "message": "Your message content",
    "sendername": "YourSender"
}
```

### Response Format
```json
{
    "status": "success",
    "message_id": "12345678",
    "cost": "1.00",
    "balance": "100.00"
}
```

## Troubleshooting

### Common Issues

1. **SMS Not Sending**
   ```
   Problem: Messages stuck as "Pending"
   Solutions:
   - Check internet connection
   - Verify API key is correct
   - Ensure account has sufficient balance
   - Check if sender name is approved
   ```

2. **Invalid Phone Numbers**
   ```
   Problem: "Invalid number format" error
   Solutions:
   - Ensure guardian phone numbers are complete
   - Use Philippine format (+639XXXXXXXXX)
   - Check for special characters in phone numbers
   ```

3. **API Authentication Failed**
   ```
   Problem: "Unauthorized" or "Invalid API key"
   Solutions:
   - Re-enter API key in configuration
   - Check API key hasn't expired
   - Verify account is active and verified
   ```

4. **Balance Issues**
   ```
   Problem: "Insufficient balance" error
   Solutions:
   - Add credits to Semaphore account
   - Check current balance in SMS configuration
   - Monitor daily SMS usage
   ```

### Error Messages
- **"SMS service is not configured"**: Configure SMS provider first
- **"No guardian phone number found"**: Ensure all students have guardians with phone numbers
- **"Invalid phone number format"**: Check phone number format
- **"Rate limit exceeded"**: Too many messages sent too quickly
- **"Account suspended"**: Contact Semaphore support

## Cost Management

### SMS Pricing (Semaphore Philippines)
- **Domestic SMS**: ₱1.00 - ₱2.50 per message
- **Bulk Discounts**: Available for high-volume users
- **Sender Name Registration**: One-time fee for custom sender names

### Cost Optimization
1. **Template Optimization**: Keep messages concise
2. **Duplicate Prevention**: System prevents duplicate sends
3. **Error Handling**: Failed messages don't consume credits
4. **Batch Processing**: Efficient bulk messaging for announcements

### Monitoring Usage
- **Daily Reports**: Track SMS count per day
- **Balance Alerts**: Monitor account balance
- **Cost Analysis**: Review SMS logs for usage patterns
- **Budget Planning**: Estimate monthly SMS costs

## Security Features

### Data Protection
- **API Key Encryption**: Secure storage of API credentials
- **Audit Trail**: Complete SMS sending history
- **Access Control**: Only administrators can configure SMS
- **Phone Number Validation**: Prevents sending to invalid numbers

### Privacy Compliance
- **Guardian Consent**: Only sends to registered guardian numbers
- **Data Retention**: SMS logs for accountability
- **Secure Transmission**: HTTPS for all API communications
- **No Sensitive Data**: Messages contain only attendance information

## Advanced Features

### Bulk Messaging
```csharp
// Send announcements to all guardians
var announcement = "School will be closed tomorrow due to weather conditions.";
var result = await notificationService.SendBulkAnnouncementAsync(announcement);
```

### Custom Templates
- **Personalization**: Include student name, time, date
- **Multi-language Support**: Customize messages for different languages
- **Emergency Templates**: Special messages for urgent situations

### Integration Points
- **RFID System**: Automatic triggers on card scan
- **Manual Sending**: Administrator-initiated messages
- **Scheduled Messages**: Future enhancement for reminders
- **Event-Driven**: Triggers based on system events

## Maintenance and Support

### Regular Tasks
- **Weekly**: Monitor SMS balance and usage
- **Monthly**: Review SMS logs for failed messages
- **Quarterly**: Update guardian phone numbers
- **Annually**: Review and optimize message templates

### Support Resources
- **Semaphore Support**: [https://semaphore.co/support](https://semaphore.co/support)
- **API Documentation**: [https://docs.semaphore.co/](https://docs.semaphore.co/)
- **System Logs**: Check Windows Event Log for application errors
- **Database Logs**: Review SMSLogs table for detailed history

For technical support with the integration, ensure you have:
1. Semaphore account details and API key
2. Error messages from SMS Configuration form
3. Sample SMS log entries from the database
4. Network connectivity test results