using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestQRApp.Data;
using TestQRApp.Models.DTOs;
using TestQRApp.Services.interfaces;

namespace TestQRApp.Services;

public class AuthService(AppDbContext db, IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    // 1. Вход для персонала (Админ/Модератор)
    public async Task<AuthResultDto> LoginStaffAsync(string login, string password)
    {
        var staff = await db.Staffs.FirstOrDefaultAsync(s => s.Login == login);
        if (staff == null)
        {
            // Передаем 4 параметра под твой обновленный рекорд DTO
            return new AuthResultDto(false, "Пользователь не найден", null, null);
        }

        // Проверяем хэш пароля
        var verificationResult = _passwordHasher.VerifyHashedPassword(staff, staff.PasswordHash, password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return new AuthResultDto(false, "Неверный пароль", null, null);
        }

        // Создаем Claims для куки
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, staff.Id.ToString()),
            new Claim(ClaimTypes.Name, staff.Login),
            new Claim(ClaimTypes.Role, staff.Role.ToString()),
            new Claim("FirstName", staff.FirstName)
        };

        await SignInAsync(claims);

        // Успех! Передаем текст в поле ErrorMessage
        return new AuthResultDto(true, "Успешный вход!", staff.Role, staff.FirstName);
    }

    // 2. Вход для клиента (с привязкой к девайсу)
    public async Task<ClientAuthResultDto> LoginClientAsync(string login, string password, string deviceId)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Login == login);
        if (client == null)
        {
            return new ClientAuthResultDto(false, "Клиент не найден", null);
        }

        // Проверяем пароль
        var verificationResult = _passwordHasher.VerifyHashedPassword(client, client.PasswordHash, password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return new ClientAuthResultDto(false, "Неверный пароль", null);
        }

        // Логика привязки устройства (Device ID)
        if (string.IsNullOrEmpty(client.ActiveDeviceId))
        {
            client.ActiveDeviceId = deviceId;
            await db.SaveChangesAsync();
        }
        else if (client.ActiveDeviceId != deviceId)
        {
            return new ClientAuthResultDto(false, "Этот аккаунт привязан к другому устройству!", null);
        }

        // Генерируем куку для клиента
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
            new Claim(ClaimTypes.Name, client.Login),
            new Claim(ClaimTypes.Role, "Client"),
            new Claim("DeviceId", deviceId)
        };

        await SignInAsync(claims);

        return new ClientAuthResultDto(true, "Успешный вход!", client.Id);
    }

    // 3. Сброс устройства (для Админа/Модератора)
    public async Task<bool> ResetClientDeviceAsync(Guid clientId)
    {
        var client = await db.Clients.FindAsync(clientId);
        if (client == null) return false;

        client.ActiveDeviceId = null;
        await db.SaveChangesAsync();
        return true;
    }

    // 4. Выход (очистка Cookie)
    public async Task LogoutAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    // Вспомогательный метод для записи куки в браузер
    private async Task SignInAsync(List<Claim> claims)
    {
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
        }
    }
    // 5. Смена пароля для персонала (Админ/Модератор)
    public async Task<bool> ChangeStaffPasswordAsync(Guid staffId, ChangePasswordDto dto)
    {
        var staff = await db.Staffs.FindAsync(staffId);
        if (staff == null) return false;

        // Проверяем текущий пароль
        var verificationResult = _passwordHasher.VerifyHashedPassword(staff, staff.PasswordHash, dto.OldPassword);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return false;
        }

        // Хешируем новый пароль и сохраняем
        staff.PasswordHash = _passwordHasher.HashPassword(staff, dto.NewPassword);
        await db.SaveChangesAsync();
        return true;
    }

    // 6. Смена пароля для обычного клиента
    public async Task<bool> ChangeClientPasswordAsync(Guid clientId, ChangePasswordDto dto)
    {
        var client = await db.Clients.FindAsync(clientId);
        if (client == null) return false;

        // Проверяем текущий пароль
        var verificationResult = _passwordHasher.VerifyHashedPassword(client, client.PasswordHash, dto.OldPassword);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return false;
        }

        // Хешируем новый пароль и сохраняем
        client.PasswordHash = _passwordHasher.HashPassword(client, dto.NewPassword);
        await db.SaveChangesAsync();
        return true;
    }
}