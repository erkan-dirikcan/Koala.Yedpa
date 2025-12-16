namespace Koala.Yedpa.Core.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
}