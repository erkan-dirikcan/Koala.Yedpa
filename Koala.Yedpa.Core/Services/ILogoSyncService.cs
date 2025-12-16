using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Services;

public interface ILogoSyncService
{
    Task<ResponseDto<string>> SyncXt001211Async(string firm, string? userId = null);
}