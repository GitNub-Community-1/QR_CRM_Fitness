using Microsoft.EntityFrameworkCore;
using TestQRApp.Data;
using TestQRApp.Models; 
using TestQRApp.Models.DTOs;
using TestQRApp.Services.interfaces;

namespace TestQRApp.Services;

public class AccessControlService(AppDbContext db, IQrCodeService qrCodeService) : IAccessControlService
{
    public async Task<AccessResultDto> ProcessEntryAsync(string qrToken)
    {
        // 1. Валидация и расшифровка токена
        var (isValidToken, clientId) = await qrCodeService.ValidateAndDecodeTokenAsync(qrToken);
        
        if (!isValidToken || clientId == Guid.Empty)
        {
            return new AccessResultDto(
                Success: false, 
                Message: "Ошибка: Недействительный или просроченный QR-код", 
                ClientName: null
            );
        }

        // 2. Поиск клиента в базе
        var client = await db.Clients.FindAsync(clientId);
        if (client == null)
        {
            return new AccessResultDto(
                Success: false, 
                Message: "Ошибка: Клиент не найден в системе", 
                ClientName: null
            );
        }

        string clientFullName = $"{client.LastName} {client.FirstName}".Trim();

        // 3. Поиск активного абонемента на текущую дату
        var today = DateTime.UtcNow.Date;
        
        var activeSubscription = await db.UserSubscriptions
            .Include(us => us.SubscriptionType)
            .FirstOrDefaultAsync(us => 
                us.ClientId == clientId && 
                us.StartDate <= today && 
                us.EndDate >= today &&
                us.IsActive);

        if (activeSubscription == null)
        {
            // Лог сразу сохранится в базу благодаря исправленному методу
            await LogAccessAttemptAsync(clientId, false, "Нет активного абонемента на текущую дату");
            return new AccessResultDto(false, "Доступ запрещен: Нет активного абонемента", clientFullName);
        }

        // 4. Проверка лимита оставшихся занятий
        if (!activeSubscription.SubscriptionType.IsUnlimited)
        {
            if (activeSubscription.RemainingVisits <= 0)
            {
                await LogAccessAttemptAsync(clientId, false, "Закончились доступные посещения");
                return new AccessResultDto(false, "Доступ запрещен: Закончились визиты", clientFullName);
            }
            
            // Списываем один визит в памяти трекера EF Core
            activeSubscription.RemainingVisits--;
        }

        // 5. Защита от повторного сканирования в течение 5 минут (Double-scan protection)
        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
        var recentLog = await db.AccessLogs
            .AnyAsync(l => l.ClientId == clientId && l.IsSuccess && l.ScanTime >= fiveMinutesAgo);

        if (recentLog)
        {
            return new AccessResultDto(false, "Внимание: Повторный проход за короткое время!", clientFullName);
        }

        // 6. Фиксация успешного прохода
        string successNote = activeSubscription.SubscriptionType.IsUnlimited 
            ? "Успешный вход. Абонемент: Безлимит" 
            : $"Успешный вход. Осталось визитов: {activeSubscription.RemainingVisits}";

        await LogAccessAttemptAsync(clientId, true, successNote);

        // Сохраняем списание визита абонемента (сам лог уже внутри базы)
        await db.SaveChangesAsync();

        return new AccessResultDto(
            Success: true, 
            Message: $"Добро пожаловать! Проход разрешен. Абонемент: {activeSubscription.SubscriptionType.Name}", 
            ClientName: clientFullName
        );
    }

    /// <summary>
    /// Вспомогательный метод для мгновенной записи логов в базу данных
    /// </summary>
    private async Task LogAccessAttemptAsync(Guid clientId, bool isSuccess, string note)
    {
        var log = new AccessLog
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ScanTime = DateTime.UtcNow,
            IsSuccess = isSuccess,
            Note = note
        };

        await db.AccessLogs.AddAsync(log);
        await db.SaveChangesAsync(); // Гарантирует сохранение лога до выхода из основного метода
    }
}