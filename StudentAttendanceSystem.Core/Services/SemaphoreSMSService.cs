using System.Text;
using System.Text.Json;
using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Core.Models;
using SMSStatus = StudentAttendanceSystem.Core.Models.SMSStatus;

namespace StudentAttendanceSystem.Core.Services
{
    public class SemaphoreSMSService : ISMSService
    {
        private readonly HttpClient _httpClient;
        private readonly SMSConfiguration _configuration;
        private readonly Func<int?, string, string, SMSStatus, string?, Task<int>> _logSMS;

        public SMSConfiguration CurrentConfig { get; }
        public Func<int?, string, string, SMSStatus, string?, string?, Task<int>> LogSMSAsync { get; }

        public event EventHandler<SMSStatusEventArgs>? SMSStatusChanged;

        public SemaphoreSMSService(SMSConfiguration configuration, Func<int?, string, string, SMSStatus, string?, Task<int>> logSMS)
        {
            _configuration = configuration;
            _logSMS = logSMS;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {configuration.ApiKey}");
        }

        public SemaphoreSMSService(SMSConfiguration currentConfig, Func<int?, string, string, SMSStatus, string?, string?, Task<int>> logSMSAsync)
        {
            CurrentConfig = currentConfig;
            LogSMSAsync = logSMSAsync;
        }

        public async Task<SMSResult> SendSMSAsync(string phoneNumber, string message, int? studentId = null)
        {
            try
            {
                // Format phone number (ensure it starts with +63 for Philippines)
                var formattedNumber = FormatPhoneNumber(phoneNumber);

                var payload = new
                {
                    number = formattedNumber,
                    message = message,
                    sendername = _configuration.SenderName
                };


                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Log SMS as pending
                var logId = await _logSMS(studentId, formattedNumber, message, SMSStatus.Pending, null);

                var response = await _httpClient.PostAsync(_configuration.ApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var semaphoreResponse = JsonSerializer.Deserialize<SemaphoreResponse>(responseContent);

                    if (semaphoreResponse?.Status?.Equals("Success", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // Update log as sent
                        await UpdateSMSLog(logId, SMSStatus.Sent, responseContent);

                        var result = new SMSResult
                        {
                            Success = true,
                            MessageId = semaphoreResponse.MessageId ?? Guid.NewGuid().ToString(),
                            SentAt = DateTime.Now,
                            Status = (Interfaces.SMSStatus)SMSStatus.Sent,
                            LogId = logId,
                            Cost = semaphoreResponse.Cost
                        };

                        OnSMSStatusChanged(result.MessageId, SMSStatus.Sent, "SMS sent successfully");
                        return result;
                    }
                    else
                    {
                        var errorMessage = semaphoreResponse?.Message ?? "Unknown error from Semaphore";
                        await UpdateSMSLog(logId, SMSStatus.Failed, errorMessage);

                        return new SMSResult
                        {
                            Success = false,
                            ErrorMessage = errorMessage,
                            Status = (Interfaces.SMSStatus)SMSStatus.Failed,
                            LogId = logId,
                            SentAt = DateTime.Now
                        };
                    }
                }
                else
                {
                    var errorMessage = $"HTTP Error: {response.StatusCode} - {responseContent}";
                    await UpdateSMSLog(logId, SMSStatus.Failed, errorMessage);

                    return new SMSResult
                    {
                        Success = false,
                        ErrorMessage = errorMessage,
                        Status = (Interfaces.SMSStatus)SMSStatus.Failed,
                        LogId = logId,
                        SentAt = DateTime.Now
                    };
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Exception: {ex.Message}";
                var logId = await _logSMS(studentId, phoneNumber, message, SMSStatus.Failed, errorMessage);

                return new SMSResult
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    Status = (Interfaces.SMSStatus)SMSStatus.Failed,
                    LogId = logId,
                    SentAt = DateTime.Now
                };
            }
        }

        public async Task<SMSResult> SendBulkSMSAsync(List<SMSRequest> requests)
        {
            var results = new List<SMSResult>();

            foreach (var request in requests)
            {
                var result = await SendSMSAsync(request.PhoneNumber, request.Message, request.StudentId);
                results.Add(result);

                // Small delay between sends to avoid rate limiting
                await Task.Delay(100);
            }

            var overallSuccess = results.All(r => r.Success);
            var failedCount = results.Count(r => !r.Success);

            return new SMSResult
            {
                Success = overallSuccess,
                MessageId = $"BULK_{DateTime.Now:yyyyMMddHHmmss}",
                ErrorMessage = failedCount > 0 ? $"{failedCount} messages failed to send" : string.Empty,
                SentAt = DateTime.Now,
                Status = (Interfaces.SMSStatus)(overallSuccess ? SMSStatus.Sent : SMSStatus.Failed)
            };
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Semaphore account info endpoint to test connection
                var accountUrl = "https://api.semaphore.co/api/v4/account";
                var response = await _httpClient.GetAsync(accountUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var accountInfo = JsonSerializer.Deserialize<SemaphoreAccountResponse>(content);
                    return !string.IsNullOrEmpty(accountInfo?.AccountName);
                }
                return false;

            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                var accountUrl = "https://api.semaphore.co/api/v4/account";
                var response = await _httpClient.GetAsync(accountUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var accountInfo = JsonSerializer.Deserialize<SemaphoreAccountResponse>(content);
                    return accountInfo?.CreditBalance ?? 0;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters
            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Handle different Philippine number formats
            if (digitsOnly.StartsWith("63"))
            {
                return "+" + digitsOnly; // Already has country code
            }
            else if (digitsOnly.StartsWith("09"))
            {
                return "+63" + digitsOnly.Substring(1); // Replace 0 with +63
            }
            else if (digitsOnly.Length == 10 && digitsOnly.StartsWith("9"))
            {
                return "+63" + digitsOnly; // Add +63 prefix
            }
            else if (digitsOnly.Length == 11 && digitsOnly.StartsWith("09"))
            {
                return "+63" + digitsOnly.Substring(1); // Standard format
            }

            // Default: assume it needs +63 prefix
            return "+63" + digitsOnly;
        }

        private async Task UpdateSMSLog(int logId, SMSStatus status, string? providerResponse)
        {
            // This would typically update the database log
            // Implementation depends on your data access layer
            await Task.CompletedTask;
        }

        private void OnSMSStatusChanged(string messageId, SMSStatus status, string message)
        {
            SMSStatusChanged?.Invoke(this, new SMSStatusEventArgs
            {
                MessageId = messageId,
                Status = (Interfaces.SMSStatus)status,
                StatusMessage = message,
                UpdatedAt = DateTime.Now
            });
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // Semaphore API Response Models
    public class SemaphoreResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? MessageId { get; set; }
        public decimal? Cost { get; set; }
        public int? Balance { get; set; }
    }

    public class SemaphoreAccountResponse
    {
        public string? Status { get; set; }
        public decimal CreditBalance { get; set; }
        public string? AccountName { get; set; }
    }
}