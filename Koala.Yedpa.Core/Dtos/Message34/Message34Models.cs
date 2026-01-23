using System.Text.Json.Serialization;

namespace Koala.Yedpa.Core.Dtos.Message34
{
    #region Authentication Models

    public class Message34AuthenticationRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public class Message34AuthenticationResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }

    #endregion

    #region Send Transaction Email Models

    public class Message34SendTransactionEmailRequest
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("fromName")]
        public string? FromName { get; set; }

        [JsonPropertyName("fromEmail")]
        public string? FromEmail { get; set; }

        [JsonPropertyName("replyEmail")]
        public string? ReplyEmail { get; set; }

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("fileBase64")]
        public string? FileBase64 { get; set; }

        [JsonPropertyName("sendEmails")]
        public List<string> SendEmails { get; set; } = new();
    }

    public class Message34SendTransactionEmailResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }

        [JsonPropertyName("campaignId")]
        public int CampaignId { get; set; }
    }

    #endregion

    #region Send Bulk Email Models

    public class Message34SendBulkEmailRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("fromName")]
        public string? FromName { get; set; }

        [JsonPropertyName("fromEmail")]
        public string? FromEmail { get; set; }

        [JsonPropertyName("replyEmail")]
        public string? ReplyEmail { get; set; }

        [JsonPropertyName("groupIds")]
        public List<int>? GroupIds { get; set; }

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("fileBase64")]
        public string? FileBase64 { get; set; }

        [JsonPropertyName("scheduledDate")]
        public DateTime? ScheduledDate { get; set; }
    }

    public class Message34SendBulkEmailResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }

        [JsonPropertyName("campaignId")]
        public int CampaignId { get; set; }
    }

    #endregion

    #region Transfer And Send Models

    public class Message34TransferAndSendRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("fromName")]
        public string? FromName { get; set; }

        [JsonPropertyName("fromEmail")]
        public string? FromEmail { get; set; }

        [JsonPropertyName("replyEmail")]
        public string? ReplyEmail { get; set; }

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("fileBase64")]
        public string? FileBase64 { get; set; }

        [JsonPropertyName("scheduledDate")]
        public DateTime? ScheduledDate { get; set; }

        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        [JsonPropertyName("sendEmails")]
        public List<Dictionary<string, string>>? SendEmails { get; set; }
    }

    public class Message34TransferAndSendResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }

        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        [JsonPropertyName("transferLogId")]
        public int TransferLogId { get; set; }

        [JsonPropertyName("requestId")]
        public Guid RequestId { get; set; }
    }

    #endregion

    #region Email Report Models

    public class Message34CampaignDetailRequest
    {
        [JsonPropertyName("campaignId")]
        public int CampaignId { get; set; }
    }

    public class Message34CampaignDetailResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("completedCount")]
        public int CompletedCount { get; set; }

        [JsonPropertyName("successCount")]
        public int SuccessCount { get; set; }

        [JsonPropertyName("failedCount")]
        public int FailedCount { get; set; }

        [JsonPropertyName("otherCount")]
        public int OtherCount { get; set; }

        [JsonPropertyName("successRate")]
        public double SuccessRate { get; set; }

        [JsonPropertyName("failedRate")]
        public double FailedRate { get; set; }

        [JsonPropertyName("openingTotal")]
        public int OpeningTotal { get; set; }

        [JsonPropertyName("openingUniqueTotal")]
        public int OpeningUniqueTotal { get; set; }

        [JsonPropertyName("totalOpenRateByTotal")]
        public double TotalOpenRateByTotal { get; set; }

        [JsonPropertyName("totalOpenRateBySuccesed")]
        public double TotalOpenRateBySuccesed { get; set; }

        [JsonPropertyName("uniqueOpenRateByTotal")]
        public double UniqueOpenRateByTotal { get; set; }

        [JsonPropertyName("uniqueOpenRateBySuccesed")]
        public double UniqueOpenRateBySuccesed { get; set; }

        [JsonPropertyName("clicksTotal")]
        public int ClicksTotal { get; set; }

        [JsonPropertyName("clicksUnique")]
        public int ClicksUnique { get; set; }

        [JsonPropertyName("totalClickRateByTotal")]
        public double TotalClickRateByTotal { get; set; }

        [JsonPropertyName("totalClickRateByOpened")]
        public double TotalClickRateByOpened { get; set; }

        [JsonPropertyName("uniqueClickRateByTotal")]
        public double UniqueClickRateByTotal { get; set; }

        [JsonPropertyName("uniqueClickRateByOpened")]
        public double UniqueClickRateByOpened { get; set; }
    }

    #endregion
}
