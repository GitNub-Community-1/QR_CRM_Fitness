using TestQRApp.Models.DTOs;

namespace TestQRApp.Services.interfaces;

public interface IAuthService
{
    // Вход для персонала (Админ/Модератор)
    Task<AuthResultDto> LoginStaffAsync(string login, string password);

    // Вход для клиента (с привязкой к девайсу)
    Task<ClientAuthResultDto> LoginClientAsync(string login, string password, string deviceId);

    // Сброс привязанного устройства (вызывает только Админ/Модератор)
    Task<bool> ResetClientDeviceAsync(Guid clientId);

    // Выход из системы (очистка Cookie)
    Task LogoutAsync();
    Task<bool> ChangeStaffPasswordAsync(Guid staffId, ChangePasswordDto dto);
    Task<bool> ChangeClientPasswordAsync(Guid clientId, ChangePasswordDto dto);
}