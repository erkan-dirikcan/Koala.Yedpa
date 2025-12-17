using Koala.Yedpa.Core.Services;

namespace Koala.Yedpa.WebUI.Extentions;

public class LicenseHeaderHandler : DelegatingHandler
{
    private readonly ILicenseValidator _licenseValidator;

    public LicenseHeaderHandler(ILicenseValidator licenseValidator)
    {
        _licenseValidator = licenseValidator;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sKey = _licenseValidator.GetXKey(); // artık instance üzerinden

        if (!string.IsNullOrEmpty(sKey))
        {
            request.Headers.Remove("X-SKey");
            request.Headers.Add("X-SKey", sKey);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
