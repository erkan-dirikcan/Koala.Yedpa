using Koala.Yedpa.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Koala.Yedpa.WebUI.Tests;

/// <summary>
/// RoleManager/UserManager sanal (virtual) metotlara sahiptir; Moq ile
/// doğrudan mock'lanabilir ancak constructor'ları bağımlılık ister.
/// Bu yardımcı o bağımlılıkları boş mock'larla doldurur.
/// </summary>
public static class IdentityMocks
{
    public static Mock<RoleManager<AppRole>> CreateRoleManagerMock()
    {
        var store = new Mock<IRoleStore<AppRole>>();
        return new Mock<RoleManager<AppRole>>(
            store.Object,
            Array.Empty<IRoleValidator<AppRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<AppRole>>>())
        {
            CallBase = false
        };
    }

    public static Mock<UserManager<AppUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<AppUser>>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<AppUser>>>())
        {
            CallBase = false
        };
    }
}
