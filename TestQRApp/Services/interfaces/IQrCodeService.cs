namespace TestQRApp.Services.interfaces;

public interface IQrCodeService
{
    // Генерирует шифрованную строку-токен (внутри: ClientId + DeviceId + Время генерации)
    string GenerateQrToken(Guid clientId, string deviceId);

    // Превращает строку-токен в Base64 картинку для тега <img src="..." />
    string GenerateQrImageBase64(string qrToken);

    // Дешифрует и проверяет токен. Если он валидный — возвращает ClientId
    Task<(bool isValid, Guid clientId)> ValidateAndDecodeTokenAsync(string qrToken);}
