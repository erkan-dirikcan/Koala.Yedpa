using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IApiLogoSqlDataService
{
    Task<ResponseListDto<List<ClCardInfoViewModel>>> GetAllClCardInfoAsync(int perPage,int pageNo);
    Task<ResponseListDto<List<ClCardInfoViewModel>>> WhereClCardInfoAsync(ClCardInfoSearchViewModel searchModel, int perPage, int pageNo);
    Task<ResponseDto<List<StatementSummeryViewModel>>> GetClsStatementsSummertAsync();
    Task<ResponseDto<List<ClCardStatementViewModel>>> GetClCardStatementAsync(string clCode);
    Task<ResponseDto<(string ClientCode, long ClientRef)>> GetClientInfoByWorkplaceCodeAsync(string workplaceCode);
    Task<ResponseDto<Dictionary<string, (string ClientCode, long ClientRef)>>> GetClientInfoByWorkplaceCodesAsync(List<string> workplaceCodes);
    Task<ResponseDto<Dictionary<string, List<WorkplaceCurrentAccounts>>>> GetWorkplaceCurrentAccountsAsync();
    Task<ResponseListDto<List<PendingInvoiceViewModel>>> GetPendingInvoicesAsync(int perPage, int pageNo);
    Task<ResponseListDto<List<PendingInvoiceViewModel>>> SearchPendingInvoicesAsync(PendingInvoiceSearchViewModel searchModel, int perPage, int pageNo);
    Task<ResponseDto<ClCardStatementDetailedViewModel>> GetClCardStatementDetailedAsync(string clCode);
    Task<ResponseListDto<List<CustomerListWithBalanceViewModel>>> GetCustomerListWithBalanceAsync(string? specode2, int perPage, int pageNo);
}