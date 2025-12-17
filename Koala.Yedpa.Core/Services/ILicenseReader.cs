namespace Koala.Yedpa.Core.Services;

public interface ILicenseReader
{
    Task<string?> GetCustomerCodeAsync();     // X-SKey
    Task<string?> GetApplicationIdAsync();    // API çağrıları için
    Task<string?> GetLogoClientIdAsync();     // Logo API ClientId
    Task<string?> GetLogoClientSecretAsync(); // Logo API ClientSecret
}