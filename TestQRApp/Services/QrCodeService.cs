using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration; // Нужно для работы с конфигурацией
using QRCoder;
using TestQRApp.Services.interfaces;

namespace TestQRApp.Services;

// Используем Primary Constructor: внедряем IConfiguration прямо в объявлении класса
public class QrCodeService(IConfiguration configuration) : IQrCodeService
{
    // Читаем значения из appsettings.json. 
    // Если в конфиге пусто — выкидываем ошибку (защита, чтобы продакшен не упал молча)
    private readonly string _encryptionKey = configuration["QrSettings:EncryptionKey"] 
        ?? throw new InvalidOperationException("QR EncryptionKey не настроен в appsettings.json");
        
    private readonly string _encryptionIv = configuration["QrSettings:EncryptionIv"] 
        ?? throw new InvalidOperationException("QR EncryptionIv не настроен в appsettings.json");

    // 1. Генерирует шифрованную строку-токен
    public string GenerateQrToken(Guid clientId, string deviceId)
    {
        string rawData = $"{clientId}|{deviceId}|{DateTime.UtcNow:o}";
        return EncryptString(rawData);
    }

    // 2. Превращает строку-токен в Base64 картинку
    public string GenerateQrImageBase64(string qrToken)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrToken, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(qrCodeBytes);
    }

    // 3. Дешифрует и проверяет токен
    public async Task<(bool isValid, Guid clientId)> ValidateAndDecodeTokenAsync(string qrToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(qrToken)) return (false, Guid.Empty);

            string decryptedData = DecryptString(qrToken);
            
            string[] parts = decryptedData.Split('|');
            if (parts.Length != 3) return (false, Guid.Empty);

            if (!Guid.TryParse(parts[0], out Guid clientId)) return (false, Guid.Empty);
            string deviceId = parts[1];
            if (!DateTime.TryParse(parts[2], out DateTime creationTime)) return (false, Guid.Empty);

            // Проверка TTL (2 минуты)
            if (DateTime.UtcNow - creationTime.ToUniversalTime() > TimeSpan.FromMinutes(2))
            {
                return (false, Guid.Empty); 
            }

            return await Task.FromResult((true, clientId));
        }
        catch
        {
            return (false, Guid.Empty);
        }
    }

    #region Криптографические хелперы (AES-256)

    private string EncryptString(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_encryptionKey); // Используем динамический ключ
        aes.IV = Encoding.UTF8.GetBytes(_encryptionIv);   // Используем динамический IV

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    private string DecryptString(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_encryptionKey); // Используем динамический ключ
        aes.IV = Encoding.UTF8.GetBytes(_encryptionIv);   // Используем динамический IV

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        
        return sr.ReadToEnd();
    }

    #endregion
}