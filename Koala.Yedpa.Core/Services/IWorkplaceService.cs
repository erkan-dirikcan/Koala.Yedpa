using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IWorkplaceService
{
    Task<ResponseDto<List<WorkplaceListViewModel>>> GetAllAsync();
    Task<ResponseDto<WorkplaceDetailViewModel>> GetByIdAsync(string id);
    Task<ResponseDto<WorkplaceDetailViewModel>> GetByCodeAsync(string code);
    Task<ResponseListDto<List<WorkplaceListViewModel>>> GetPagedListAsync(int start, int length, string? searchValue = null, string? orderColumn = null, bool orderAscending = true);
    Task<ResponseDto<WorkplaceDetailViewModel>> UpdateAsync(WorkplaceDetailViewModel model);
    Task<ResponseDto<WorkplaceBulkEmailResultViewModel>> SendBulkBudgetEmailsAsync(int year);
    Task<ResponseDto<byte[]>> GenerateBudgetExcelAsync(int year);
    Task<ResponseDto<string>> GenerateQRCodeForWorkplaceAsync(string id);
    Task<ResponseDto<List<string>>> GenerateBulkQRCodesAsync();
}

public class WorkplaceBulkEmailResultViewModel
{
    public int TotalWorkplaces { get; set; }
    public int TotalEmailsSent { get; set; }
    public int TotalEmailsFailed { get; set; }
    public List<WorkplaceEmailResult> SuccessfulEmails { get; set; } = new();
    public List<WorkplaceEmailResult> FailedEmails { get; set; } = new();
}

public class WorkplaceEmailResult
{
    public string WorkplaceCode { get; set; } = string.Empty;
    public string WorkplaceName { get; set; } = string.Empty;
    public string CurrentAccountName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
