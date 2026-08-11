using Koala.Yedpa.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
}
