namespace Koala.Yedpa.Core.Services;

public interface ICryptoService
{
    Task<string> EncryptAsync(string plainText);
    Task<string> DecryptAsync(string cipherText);
}