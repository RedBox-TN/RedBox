using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace RedBox.Encryption_utility;

public class EncryptionUtility : IEncryptionUtility
{
    public async Task<(byte[] EncData, byte[] Iv)> EncryptAsync(string clearText, byte[] key, DateTime? expiration,
        int keySize)
    {
        using var aes = Aes.Create();
        aes.KeySize = keySize;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;

        var clearTextFinal =
            Encoding.UTF8.GetBytes(expiration.HasValue ? $"{clearText}#{expiration.Value.ToString("ddMMyyyyHHmmss")}" : clearText);

        using MemoryStream output = new();
        await using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await cryptoStream.WriteAsync(clearTextFinal);
        await cryptoStream.FlushFinalBlockAsync();

        return (output.ToArray(), aes.IV);
    }

    public async Task<byte[]> DecryptAsync(byte[] encrypted, byte[] key, byte[] iv, int keySize)
    {
        using var aes = Aes.Create();
        aes.KeySize = keySize;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using MemoryStream input = new(encrypted);
        await using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using MemoryStream output = new();
        await cryptoStream.CopyToAsync(output);
        return output.ToArray();
    }

    public byte[] DeriveKey(string password, int keySize)
    {
        using var argon2 = new Argon2id(Encoding.UTF32.GetBytes(password));
        argon2.Iterations = 20;
        argon2.MemorySize = 512;
        argon2.DegreeOfParallelism = 4;

        return argon2.GetBytes(keySize / 8);
    }
}