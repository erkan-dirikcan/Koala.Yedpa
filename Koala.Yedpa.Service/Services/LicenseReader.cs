using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Services;

public class LicenseReader : ILicenseReader
{
    private string? _sKey;
    private string? _appId;
    private string? _logoClientId;
    private string? _logoClientSecret;

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

    public async Task<string?> GetLogoClientIdAsync()
    {
        if (_logoClientId != null) return _logoClientId;

        var payload = LicenseFileHelper.ReadLicensePayload();
        _logoClientId = payload?.LogoClientId;
        return _logoClientId;
    }

    public async Task<string?> GetLogoClientSecretAsync()
    {
        if (_logoClientSecret != null) return _logoClientSecret;

        var payload = LicenseFileHelper.ReadLicensePayload();
        _logoClientSecret = payload?.LogoClientSecret;
        return _logoClientSecret;
    }
}