namespace Koala.Yedpa.Core.Exceptions;

/// <summary>
/// Crypto API lisans hatası için özel exception
/// </summary>
public class CryptoLicenseException : Exception
{
    public CryptoLicenseException(string message) : base(message)
    {
    }

    public CryptoLicenseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
