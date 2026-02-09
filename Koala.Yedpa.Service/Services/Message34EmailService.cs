using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Message34;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class Message34EmailService : IMessage34EmailService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<Message34EmailService> _logger;
    private string? _authToken;
    private DateTime? _tokenExpiry;
    private Message34SettingsViewModel? _cachedSettings;
    private DateTime? _settingsCacheExpiry;

    public Message34EmailService(
        HttpClient httpClient,
        ISettingsService settingsService,
        ILogger<Message34EmailService> logger)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        _logger = logger;
    }

    private async Task<Message34SettingsViewModel?> GetSettingsAsync()
    {
        // Ayarlar cache'de varsa ve 5 dakikadan daha yeniyse cache'den dön
        if (_cachedSettings != null && _settingsCacheExpiry.HasValue && DateTime.Now < _settingsCacheExpiry.Value)
        {
            return _cachedSettings;
        }

        var settingsResponse = await _settingsService.GetMessage34SettingsAsync();
        if (!settingsResponse.IsSuccess || settingsResponse.Data == null)
        {
            _logger.LogError("Message34 ayarları alınamadı");
            return null;
        }

        _cachedSettings = settingsResponse.Data;
        _settingsCacheExpiry = DateTime.Now.AddMinutes(5); // 5 dakika cache

        // BaseAddress'i güncelle
        if (!string.IsNullOrEmpty(_cachedSettings.BaseApi))
        {
            _httpClient.BaseAddress = new Uri(_cachedSettings.BaseApi);
        }

        return _cachedSettings;
    }

    public async Task<bool> AuthenticateAsync()
    {
        // Token hala geçerliyse yenileme
        if (_tokenExpiry.HasValue && DateTime.Now < _tokenExpiry.Value && !string.IsNullOrEmpty(_authToken))
        {
            return true;
        }

        var settings = await GetSettingsAsync();
        if (settings == null)
        {
            return false;
        }

        try
        {
            var authRequest = new Message34AuthenticationRequest
            {
                Username = settings.UserName,
                Password = settings.Password
            };

            var json = JsonSerializer.Serialize(authRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v2/Authentication/authenticate", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Message34 authentication failed. Status: {Status}", response.StatusCode);
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<Message34AuthenticationResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (authResponse != null && authResponse.Success)
            {
                // Token'ı response body'den Value property'sinden al
                _authToken = authResponse.Value;

                if (!string.IsNullOrEmpty(_authToken))
                {
                    // Token'ı 50 dakika geçerli say (varsayılan olarak)
                    _tokenExpiry = DateTime.Now.AddMinutes(50);

                    // HttpClient'a Authorization header ekle
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _authToken);

                    _logger.LogInformation("Message34 authentication successful");
                    return true;
                }
            }

            _logger.LogError("Message34 authentication failed - no token received");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Message34 authentication");
            return false;
        }
    }

    public async Task<ResponseDto<int?>> SendTransactionEmailAsync(EmailDto email, List<string>? toEmails = null)
    {
        if (!await EnsureAuthenticated())
        {
            return ResponseDto<int?>.FailData(500, "Authentication failed", "Message34 authentication failed", true);
        }

        var settings = await GetSettingsAsync();
        if (settings == null)
        {
            return ResponseDto<int?>.FailData(500, "Settings not available", "Message34 settings not available", true);
        }

        try
        {
            // Email listesi hazırla
            var emailList = new List<string>();// toEmails ?? new List<string> { email.Email };
            emailList.Add("erkan@sistem-bilgisayar.com.tr");
            emailList.Add("simsek_ozcan@hotmail.com");
            emailList.Add("tahsilat@yedpa.com.tr");
            emailList.Add("adegimli@yedpa.com.tr");
            // Attachment varsa base64'e çevir
            string? fileBase64 = null;
            string? fileName = null;

            if (email.Attachments != null && email.Attachments.Any())
            {
                var attachment = email.Attachments.First();
                fileName = attachment.FileName;
                fileBase64 = Convert.ToBase64String(attachment.Content);
            }

       

            var request = new Message34SendTransactionEmailRequest
            {
                Subject = email.Title,
                Body = email.Content,
                FromName = email.FromName ?? settings.SendName,
                FromEmail = email.FromEmail ?? settings.SendMail,
                ReplyEmail = email.ReplyEmail ?? settings.ReplyEmail,
                FileName = fileName,
                FileBase64 = fileBase64,
                SendEmails = emailList
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v2/EmailDelivery/sendtransactionemail", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Message34SendTransactionEmailResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null && result.Success)
            {
                _logger.LogInformation("Transactional email sent successfully. CampaignId: {CampaignId}", result.CampaignId);
                return ResponseDto<int?>.SuccessData(200, "Email sent successfully", result.CampaignId);
            }
            else
            {
                var errors = result?.Errors != null ? string.Join(", ", result.Errors) : "Unknown error";
                _logger.LogError("Failed to send transactional email. Errors: {Errors}", errors);
                return ResponseDto<int?>.FailData(500, "Failed to send email", errors, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending transactional email");
            return ResponseDto<int?>.FailData(500, "Error sending email", ex.Message, true);
        }
    }

    public async Task<ResponseDto<int?>> SendBulkEmailAsync(Message34SendBulkEmailRequest request)
    {
        if (!await EnsureAuthenticated())
        {
            return ResponseDto<int?>.FailData(500, "Authentication failed", "Message34 authentication failed", true);
        }

        var settings = await GetSettingsAsync();
        if (settings == null)
        {
            return ResponseDto<int?>.FailData(500, "Settings not available", "Message34 settings not available", true);
        }

        try
        {
            // Default values
            request.FromName ??= settings.SendName;
            request.FromEmail ??= settings.SendMail;
            request.ReplyEmail ??= settings.ReplyEmail;

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v2/EmailDelivery/sendbulkemail", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Message34SendBulkEmailResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null && result.Success)
            {
                _logger.LogInformation("Bulk email sent successfully. CampaignId: {CampaignId}", result.CampaignId);
                return ResponseDto<int?>.SuccessData(200, "Bulk email sent successfully", result.CampaignId);
            }
            else
            {
                var errors = result?.Errors != null ? string.Join(", ", result.Errors) : "Unknown error";
                _logger.LogError("Failed to send bulk email. Errors: {Errors}", errors);
                return ResponseDto<int?>.FailData(500, "Failed to send bulk email", errors, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk email");
            return ResponseDto<int?>.FailData(500, "Error sending bulk email", ex.Message, true);
        }
    }

    public async Task<ResponseDto<int?>> TransferAndSendEmailAsync(Message34TransferAndSendRequest request)
    {
        if (!await EnsureAuthenticated())
        {
            return ResponseDto<int?>.FailData(500, "Authentication failed", "Message34 authentication failed", true);
        }

        var settings = await GetSettingsAsync();
        if (settings == null)
        {
            return ResponseDto<int?>.FailData(500, "Settings not available", "Message34 settings not available", true);
        }

        try
        {
            // Default values
            request.FromName ??= settings.SendName;
            request.FromEmail ??= settings.SendMail;
            request.ReplyEmail ??= settings.ReplyEmail;

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v2/EmailDelivery/transferandsend", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Message34TransferAndSendResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null && result.Success)
            {
                _logger.LogInformation("Transfer and send completed successfully. GroupId: {GroupId}, CampaignId: {CampaignId}",
                    result.GroupId, result.TransferLogId);
                return ResponseDto<int?>.SuccessData(200, "Transfer and send completed successfully", result.TransferLogId);
            }
            else
            {
                var errors = result?.Errors != null ? string.Join(", ", result.Errors) : "Unknown error";
                _logger.LogError("Failed to transfer and send. Errors: {Errors}", errors);
                return ResponseDto<int?>.FailData(500, "Failed to transfer and send", errors, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in transfer and send");
            return ResponseDto<int?>.FailData(500, "Error in transfer and send", ex.Message, true);
        }
    }

    public async Task<ResponseDto<Message34CampaignDetailResponse?>> GetCampaignDetailsAsync(int campaignId)
    {
        if (!await EnsureAuthenticated())
        {
            return ResponseDto<Message34CampaignDetailResponse>.FailData(500, "Authentication failed", "Message34 authentication failed", true);
        }

        try
        {
            var request = new Message34CampaignDetailRequest { CampaignId = campaignId };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v2/EmailReport/campaigndetails", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Message34CampaignDetailResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null && result.Success)
            {
                return ResponseDto<Message34CampaignDetailResponse?>.SuccessData(200, "Campaign details retrieved", result);
            }
            else
            {
                var errors = result?.Errors != null ? string.Join(", ", result.Errors) : "Unknown error";
                _logger.LogError("Failed to get campaign details. Errors: {Errors}", errors);
                return ResponseDto<Message34CampaignDetailResponse?>.FailData(500, "Failed to get campaign details", errors, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting campaign details");
            return ResponseDto<Message34CampaignDetailResponse?>.FailData(500, "Error getting campaign details", ex.Message, true);
        }
    }

    private async Task<bool> EnsureAuthenticated()
    {
        if (_tokenExpiry.HasValue && DateTime.Now < _tokenExpiry.Value && !string.IsNullOrEmpty(_authToken))
        {
            return true;
        }

        return await AuthenticateAsync();
    }
}

//public class Message34Settings
//{
//    public const string SectionName = "Message34";
//    public string BaseApi { get; set; } = "https://api.message34.com";
//    public string UserName { get; set; } = string.Empty;
//    public string Password { get; set; } = string.Empty;
//    public string SendName { get; set; } = "Yedpa";
//    public string SendMail { get; set; } = string.Empty;
//    public string? ReplyEmail { get; set; }
//}
