using Microsoft.AspNetCore.Mvc;
using TestQRApp.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TestQRApp.Models.DTOs;
using TestQRApp.Models; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestQRApp.Models.Entity_s;

namespace TestQRApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // 1. Вход для Персонала (Админ/Модератор)
    [HttpPost("login-staff")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> LoginStaff([FromForm] string login, [FromForm] string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return Redirect($"/?error={Uri.EscapeDataString("Заполните все поля")}");
        }

        var result = await authService.LoginStaffAsync(login, password);

        if (!result.IsSuccess)
        {
            return Redirect($"/?error={Uri.EscapeDataString(result.ErrorMessage)}");
        }

        return Redirect("/trainers");
    }

    // 2. Вход для Клиентов (с проверкой девайса)
    [HttpPost("login-client")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> LoginClient([FromForm] string login, [FromForm] string password, [FromForm] string deviceId)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(deviceId))
        {
            return Redirect($"/?error={Uri.EscapeDataString("Некорректные данные или отсутствует ID устройства")}");
        }

        var result = await authService.LoginClientAsync(login, password, deviceId);

        if (!result.IsSuccess)
        {
            return Redirect($"/?error={Uri.EscapeDataString(result.ErrorMessage)}");
        }

        return Redirect("/client/dashboard");
    }

    // 3. Выход из системы (Logout)
    // 3. Выход из системы (Logout)
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync();
    
        // Принудительно очищаем куки в ответе
        Response.Cookies.Delete(".AspNetCore.Cookies"); // или имя твоей куки авторизации

        // Делаем Hard-Redirect (302) прямо на главную страницу входа
        return LocalRedirect("/");
    }
    // 4. Смена пароля сотрудника
    [Authorize] 
    [HttpPost("change-password-staff")]
    public async Task<IActionResult> ChangeStaffPassword([FromBody] ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest("Пароли не могут быть пустыми.");
        }

        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(nameIdentifier, out var staffId))
        {
            return Unauthorized();
        }

        var isSuccess = await authService.ChangeStaffPasswordAsync(staffId, dto);
        if (!isSuccess)
        {
            return BadRequest("Неверный старый пароль или пользователь не найден.");
        }

        return Ok("Пароль успешно изменен.");
    }

    // 5. Смена пароля клиента
    [Authorize(Roles = "Client")] 
    [HttpPost("change-password-client")]
    public async Task<IActionResult> ChangeClientPassword([FromBody] ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest("Пароли не могут быть пустыми.");
        }

        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(nameIdentifier, out var clientId))
        {
            return Unauthorized();
        }

        var isSuccess = await authService.ChangeClientPasswordAsync(clientId, dto);
        if (!isSuccess)
        {
            return BadRequest("Неверный старый пароль.");
        }

        return Ok("Пароль успешно изменен.");
    }

    // 6. Единый эндпоинт-роутер (Умный вход по одной форме)
    [HttpPost("login-router")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> LoginRouter([FromForm] string login, [FromForm] string password, [FromForm] string deviceId)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return Redirect($"/?error={Uri.EscapeDataString("Заполните все поля")}");
        }

        // Проверяем персонал
        var staffResult = await authService.LoginStaffAsync(login, password);
        if (staffResult.IsSuccess)
        {
            return Redirect("/trainers");
        }
        
        if (staffResult.ErrorMessage == "Неверный пароль")
        {
            return Redirect($"/?error={Uri.EscapeDataString(staffResult.ErrorMessage)}");
        }

        // Проверяем клиента
        var clientResult = await authService.LoginClientAsync(login, password, deviceId);
        if (clientResult.IsSuccess)
        {
            return Redirect("/client/dashboard");
        }

        return Redirect($"/?error={Uri.EscapeDataString(clientResult.ErrorMessage)}");
    }

    // 7. Полноценное создание клиента модератором/админом
    [HttpPost("/api/staff/create-client")] 
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> CreateClient(
        [FromForm] string FirstName, 
        [FromForm] string LastName, 
        [FromForm] string PhoneNumber, 
        [FromForm] string Login, 
        [FromForm] string Password,
        [FromServices] TestQRApp.Data.AppDbContext db) 
    {
        if (string.IsNullOrWhiteSpace(FirstName) || 
            string.IsNullOrWhiteSpace(LastName) || 
            string.IsNullOrWhiteSpace(PhoneNumber) || 
            string.IsNullOrWhiteSpace(Login) || 
            string.IsNullOrWhiteSpace(Password))
        {
            return Redirect($"/trainers?error={Uri.EscapeDataString("Все поля обязательны для заполнения")}");
        }

        var isLoginTaken = await db.Clients.AnyAsync(c => c.Login == Login) || 
                           await db.Staffs.AnyAsync(s => s.Login == Login);
                           
        if (isLoginTaken)
        {
            return Redirect($"/trainers?error={Uri.EscapeDataString("Этот логин уже занят")}");
        }

        var passwordHasher = new PasswordHasher<object>();
        string hashedPassword = passwordHasher.HashPassword(new object(), Password);

        var newClient = new Client
        {
            Id = Guid.NewGuid(),
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = string.Empty, 
            Age = 18, 
            Gender = 0, 
            PhoneNumber = PhoneNumber,
            Login = Login,
            PasswordHash = hashedPassword,
            ActiveDeviceId = null, 
            CreatedAt = DateTime.UtcNow,
            TrainerId = null 
        };

        try
        {
            await db.Clients.AddAsync(newClient);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return Redirect($"/trainers?error={Uri.EscapeDataString("Ошибка базы данных: " + ex.Message)}");
        }

        return Redirect($"/trainers?success={Uri.EscapeDataString("Клиент успешно создан")}");
    }
}