using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestQRApp.Data;
using TestQRApp.Models.Entity_s;
using TestQRApp.Models.Enums;

namespace TestQRApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var hasher = new PasswordHasher<object>();

        // 1. Проверяем / Создаем / Обновляем АДМИНА
        var admin = await db.Staffs.FirstOrDefaultAsync(s => s.Login == "admin");
        if (admin == null)
        {
            admin = new Staff
            {
                Id = Guid.NewGuid(),
                FirstName = "Главный",
                LastName = "Админ",
                Login = "admin",
                Role = StaffRole.Admin,
                CreatedAt = DateTimeOffset.UtcNow
            };
            admin.PasswordHash = hasher.HashPassword(admin, "123456");
            await db.Staffs.AddAsync(admin);
        }
        else
        {
            // Принудительно обновляем хеш пароля до валидного
            admin.PasswordHash = hasher.HashPassword(admin, "123456");
        }

        // 2. Проверяем / Создаем / Обновляем МОДЕРАТОРА
        var moderator = await db.Staffs.FirstOrDefaultAsync(s => s.Login == "moderator");
        if (moderator == null)
        {
            moderator = new Staff
            {
                Id = Guid.NewGuid(),
                FirstName = "Иван",
                LastName = "Модераторов",
                Login = "moderator",
                Role = StaffRole.Moderator,
                CreatedAt = DateTimeOffset.UtcNow
            };
            moderator.PasswordHash = hasher.HashPassword(moderator, "123456");
            await db.Staffs.AddAsync(moderator);
        }
        else
        {
            // Принудительно обновляем хеш пароля
            moderator.PasswordHash = hasher.HashPassword(moderator, "123456");
        }

        await db.SaveChangesAsync();
    }
}