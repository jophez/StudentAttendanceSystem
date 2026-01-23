using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StudentAttendanceSystem.Core.Interfaces;
using StudentAttendanceSystem.Core.Models;
using SMSStatus = StudentAttendanceSystem.Core.Models.SMSStatus;

namespace StudentAttendanceSystem.Core.Services
{
    public class SemaphoreSMSService : ISMSService
    {
        private readonly HttpClient _httpClient;
        private readonly SMSConfiguration _configuration;
        private readonly Func<int?, string, string, SMSStatus, string?, string?, Task<int>> _logSMS;
        private readonly Func<int, SMSStatus, string?, string?, Task<bool>> _updateSMSLog;

        public SMSConfiguration CurrentConfig => _configuration;

        public event EventHandler<SMSStatusEventArgs>? SMSStatusChanged;

        public SemaphoreSMSService(
            SMSConfiguration configuration,
            Func<int?, string, string, SMSStatus, string?, string?, Task<int>> logSMS,
            Func<int, SMSStatus, string?, string?, Task<bool>> updateSMSLog)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logSMS = logSMS ?? throw new ArgumentNullException(nameof(logSMS));
            _updateSMSLog = updateSMSLog ?? throw new ArgumentNullException(nameof(updateSMSLog));

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            // Don't add Authorization header - Semaphore uses apikey in body/query params
        }

        public async Task<SMSResult> SendSMSAsync(string phoneNumber, string message, int? studentId = null)
        {
            int logId = 0;

            try
            {
                // Format phone number (ensure it starts with 63 for Philippines)
                var formattedNumber = FormatPhoneNumber(phoneNumber);

                // Semaphore expects apikey in the body, not in headers
                var payload = new
                {
                    apikey = _configuration.ApiKey,
                    number = formattedNumber,
                    message = message,
                    sendername = _configuration.SenderName
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Log SMS as pending
                logId = await _logSMS(studentId, formattedNumber, message, SMSStatus.Pending, null, null);

                var response = await _httpClient.PostAsync(_configuration.ApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Semaphore HTTP Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Semaphore Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };

                    SemaphoreResponse? semaphoreResponse = null;

                    try
                    {
                        // CRITICAL FIX: Semaphore returns an ARRAY even for single messages
                        if (responseContent.TrimStart().StartsWith("["))
                        {
                            var responseArray = JsonSerializer.Deserialize<List<SemaphoreResponse>>(responseContent, options);
                            semaphoreResponse = responseArray?.FirstOrDefault();
                            System.Diagnostics.Debug.WriteLine($"Parsed as array, got {responseArray?.Count ?? 0} items");
                        }
                        else
                        {
                            semaphoreResponse = JsonSerializer.Deserialize<SemaphoreResponse>(responseContent, options);
                            System.Diagnostics.Debug.WriteLine("Parsed as single object");
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"JSON Parse Error: {jsonEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Raw Response: {responseContent}");

                        // If we get HTTP 200 but can't parse, treat as success with warning
                        await _updateSMSLog(logId, SMSStatus.Sent, "Warning: Could not parse response", responseContent);

                        return new SMSResult
                        {
                            Success = true,
                            MessageId = Guid.NewGuid().ToString(),
                            SentAt = DateTime.Now,
                            Status = (Interfaces.SMSStatus)SMSStatus.Sent,
                            LogId = logId,
                            ErrorMessage = "Sent but response format unexpected"
                        };
                    }

                    if (semaphoreResponse != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Response Status: {semaphoreResponse.Status}");
                        System.Diagnostics.Debug.WriteLine($"Response MessageId: {semaphoreResponse.MessageId}");
                        System.Diagnostics.Debug.WriteLine($"Response Message: {semaphoreResponse.Message}");
                    }

                    // Check for success
                    var isSuccess = semaphoreResponse != null &&
                        (semaphoreResponse.Status?.Equals("Success", StringComparison.OrdinalIgnoreCase) == true ||
                         semaphoreResponse.Status?.Equals("Queued", StringComparison.OrdinalIgnoreCase) == true ||
                         semaphoreResponse.Status?.Equals("Pending", StringComparison.OrdinalIgnoreCase) == true ||
                         !string.IsNullOrEmpty(semaphoreResponse.MessageId));

                    if (isSuccess)
                    {
                        await _updateSMSLog(logId, SMSStatus.Sent, null, responseContent);

                        var result = new SMSResult
                        {
                            Success = true,
                            MessageId = semaphoreResponse!.MessageId ?? Guid.NewGuid().ToString(),
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
                        var errorMessage = semaphoreResponse?.Message ??
                                         semaphoreResponse?.Error ??
                                         "Unknown error from Semaphore";
                        await _updateSMSLog(logId, SMSStatus.Failed, errorMessage, responseContent);

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
                    // HTTP error
                    var errorMessage = $"HTTP {response.StatusCode}";

                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            NumberHandling = JsonNumberHandling.AllowReadingFromString
                        };

                        if (responseContent.TrimStart().StartsWith("["))
                        {
                            var errorArray = JsonSerializer.Deserialize<List<SemaphoreResponse>>(responseContent, options);
                            var errorResponse = errorArray?.FirstOrDefault();
                            if (!string.IsNullOrEmpty(errorResponse?.Message))
                                errorMessage = errorResponse.Message;
                            else if (!string.IsNullOrEmpty(errorResponse?.Error))
                                errorMessage = errorResponse.Error;
                        }
                        else
                        {
                            var errorResponse = JsonSerializer.Deserialize<SemaphoreResponse>(responseContent, options);
                            if (!string.IsNullOrEmpty(errorResponse?.Message))
                                errorMessage = errorResponse.Message;
                            else if (!string.IsNullOrEmpty(errorResponse?.Error))
                                errorMessage = errorResponse.Error;
                        }
                    }
                    catch
                    {
                        errorMessage += $": {responseContent}";
                    }

                    await _updateSMSLog(logId, SMSStatus.Failed, errorMessage, responseContent);

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
                System.Diagnostics.Debug.WriteLine($"SMS Send Exception: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (logId == 0)
                {
                    logId = await _logSMS(studentId, phoneNumber, message, SMSStatus.Failed, errorMessage, null);
                }
                else
                {
                    await _updateSMSLog(logId, SMSStatus.Failed, errorMessage, null);
                }

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
                // FIXED: Semaphore requires apikey as query parameter
                var accountUrl = $"https://api.semaphore.co/api/v4/account?apikey={_configuration.ApiKey}";

                System.Diagnostics.Debug.WriteLine($"Testing connection to: {accountUrl.Replace(_configuration.ApiKey, "***")}");

                var response = await _httpClient.GetAsync(accountUrl);

                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Response Content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };
                    var accountInfo = JsonSerializer.Deserialize<SemaphoreAccountResponse>(content, options);

                    System.Diagnostics.Debug.WriteLine($"Account Name: {accountInfo?.AccountName}");
                    System.Diagnostics.Debug.WriteLine($"Credit Balance: {accountInfo?.CreditBalance}");

                    return accountInfo != null && accountInfo.CreditBalance >= 0;
                }

                System.Diagnostics.Debug.WriteLine($"Connection test failed with status: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestConnection Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                // FIXED: Add API key as query parameter
                var accountUrl = $"https://api.semaphore.co/api/v4/account?apikey={_configuration.ApiKey}";
                var response = await _httpClient.GetAsync(accountUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };
                    var accountInfo = JsonSerializer.Deserialize<SemaphoreAccountResponse>(content, options);
                    return accountInfo?.CreditBalance ?? 0;
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get Balance Error: {ex.Message}");
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
                return digitsOnly; // Already has country code
            }
            else if (digitsOnly.StartsWith("09"))
            {
                return "63" + digitsOnly.Substring(1); // Replace 0 with 63
            }
            else if (digitsOnly.Length == 10 && digitsOnly.StartsWith("9"))
            {
                return "63" + digitsOnly; // Add 63 prefix
            }
            else if (digitsOnly.Length == 11 && digitsOnly.StartsWith("09"))
            {
                return "63" + digitsOnly.Substring(1); // Standard format
            }

            // Default: assume it needs 63 prefix
            return "63" + digitsOnly;
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

    // Semaphore API Response Models with proper JSON property mappings
    public class SemaphoreResponse
    {
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }

        [JsonPropertyName("account_id")]
        public string? AccountId { get; set; }

        [JsonPropertyName("account")]
        public string? Account { get; set; }

        [JsonPropertyName("recipient")]
        public string? Recipient { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("sender_name")]
        public string? SenderName { get; set; }

        [JsonPropertyName("network")]
        public string? Network { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("cost")]
        public decimal? Cost { get; set; }

        [JsonPropertyName("balance")]
        public decimal? Balance { get; set; }
    }

    public class SemaphoreAccountResponse
    {
        [JsonPropertyName("account_id")]
        public int AccountId { get; set; }

        [JsonPropertyName("account_name")]
        public string? AccountName { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("credit_balance")]
        public decimal CreditBalance { get; set; }
    }
}