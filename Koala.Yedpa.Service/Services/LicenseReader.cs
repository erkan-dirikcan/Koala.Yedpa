using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Services;

public class LicenseReader : ILicenseReader
{
    private string? _sKey;
    private string? _appId;

    public async Task<string?> GetCustomerCodeAsync()
    {
        if (_sKey != null) return _sKey;

        var payload = LicenseFileHelper.ReadLicensePayload();
        _sKey = payload?.CustomerCode;
        return _sKey;
    }

    public async Task<string?> GetApplicationIdAsync()
    {
        if (_appId != null) return _appId;

        var payload = LicenseFileHelper.ReadLicensePayload();
        _appId = payload?.ApplicationId;
        return _appId;
    }
}