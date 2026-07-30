using Microsoft.EntityFrameworkCore;
using TestQRApp.Data;
using TestQRApp.Models.DTOs;
using TestQRApp.Services.interfaces;

namespace TestQRApp.Services;

// Используем Primary Constructor: внедряем контекст БД прямо в объявлении класса.
// Rider будет счастлив — никаких лишних приватных полей и громоздких конструкторов!
public class AnalytickcService(AppDbContext db) : IAnalytickcService
{
    // 1. Сколько людей зашло за последние 3 часа (Текущая загрузка)
    public async Task<int> GetCurrentGymLoadAsync()
    {
        // Вычисляем точку во времени: текущее время минус 3 часа
        var threeHoursAgo = DateTime.UtcNow.AddHours(-3);

        // LINQ-запрос:
        return await db.AccessLogs
            // Where: Фильтруем логи доступа. Берем только успешные входы (IsSuccess == true)
            // и только те, которые произошли позже, чем 3 часа назад (ScanTime >= threeHoursAgo)
            .Where(log => log.IsSuccess && log.ScanTime >= threeHoursAgo)
            // CountAsync: Считаем количество записей, подходящих под условия, прямо на стороне PostgreSQL
            .CountAsync();
    }

    // 2. Всего успешных посещений за сегодня
    public async Task<int> GetTodayVisitsCountAsync()
    {
        // Берем чистую дату начала сегодняшнего дня (00:00:00) в UTC
        var today = DateTime.UtcNow.Date;

        // LINQ-запрос:
        return await db.AccessLogs
            // Where: Фильтруем логи. Нам нужны только успешные проходы (IsSuccess)
            // за сегодняшний день, то есть время сканирования должно быть больше или равно 00:00 сегодняшнего дня
            .Where(log => log.IsSuccess && log.ScanTime >= today)
            // CountAsync: Агрегируем (считаем) данные в базе и возвращаем итоговое число
            .CountAsync();
    }

    // 3. Новых регистраций за текущий месяц
    public async Task<int> GetMonthlyNewClientsCountAsync()
    {
        var now = DateTime.UtcNow;
        // Создаем дату начала текущего месяца (например, 1-е число текущего месяца, 00:00:00)
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // LINQ-запрос:
        return await db.Clients
            // Where: Фильтруем таблицу клиентов. Проверяем поле даты регистрации (CreatedAt).
            // Нам нужны те, кто зарегистрировался начиная с первого числа этого месяца и по сей день
            .Where(client => client.CreatedAt >= firstDayOfMonth)
            // CountAsync: Подсчитываем новых клиентов одной быстрой SQL-командой COUNT()
            .CountAsync();
    }

    // 4. Последние N входов для живой ленты на ресепшене
    public async Task<List<RecentPassDto>> GetRecentPassesAsync(int count = 10)
    {
        // LINQ-запрос с жадной загрузкой (Eager Loading) и проекцией данных:
        return await db.AccessLogs
            // Include: Говорим EF Core сделать SQL-команду JOIN с таблицей Clients.
            // Без этого поле log.Client будет равно null, и мы не сможем узнать имя человека!
            .Include(log => log.Client)
            // OrderByDescending: Сортируем логи по времени сканирования от самых новых к старым,
            // чтобы последние зашедшие люди были вверху списка ленты
            .OrderByDescending(log => log.ScanTime)
            // Take: Ограничиваем выборку. Берем из базы только N записей (по дефолту 10),
            // чтобы не тянуть из базы миллионы старых логов
            .Take(count)
            // Select: Проекция (трансформация) данных. Превращаем тяжелую модель БД 'AccessLog'
            // в легкий и безопасный объект 'RecentPassDto', который полетит на фронтенд Blazor
            .Select(log => new RecentPassDto
            (
                log.Id,
                log.ClientId,
                // Тернарный оператор: если клиент нашелся (JOIN прошел успешно), собираем ФИО,
                // иначе пишем "Неизвестный клиент" (на случай, если данные были удалены)
                log.Client != null ? $"{log.Client.LastName} {log.Client.FirstName}".Trim() : "Неизвестный клиент",
                log.ScanTime,
                log.IsSuccess,
                log.Note
            ))
            // ToListAsync: Выполняем собранный SQL-запрос в PostgreSQL и собираем результат в List
            .ToListAsync();
    }
}
//Твоя шпаргалка по LINQ в этом сервисе:

    //.Where() — это аналог SQL-команды WHERE. Он фильтрует строки. То, что внутри него, преобразуется в условия типа AND / OR в базе данных.

    //.Include() — это аналог SQL-команды LEFT JOIN. Подгружает связанные данные из другой таблицы по внешнему ключу (ForeignKey).

    //.OrderByDescending() — аналог ORDER BY ... DESC. Разворачивает список так, чтобы свежие события были первыми.

    //.Take(N) — аналог SQL-команды LIMIT N. Обрубает хвост выборки, экономя оперативку сервера и трафик.

    //.Select() — аналог ручного перечисления полей в SELECT блоке SQL. Он вытягивает только те колонки, которые ты указал в конструкторе RecentPassDto, игнорируя ненужный мусор.
    