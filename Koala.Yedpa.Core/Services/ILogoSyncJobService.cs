namespace Koala.Yedpa.Core.Services;

public interface ILogoSyncJobService
{
    Task SyncFromLogoAsync(string? triggeredByUserId = null);
}