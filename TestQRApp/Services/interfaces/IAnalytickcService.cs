using TestQRApp.Models.DTOs;

namespace TestQRApp.Services.interfaces;

public interface IAnalytickcService
{
    // Сколько людей зашло за последние 3 часа
    Task<int> GetCurrentGymLoadAsync();

    // Всего успешных посещений за сегодня
    Task<int> GetTodayVisitsCountAsync();

    // Новых регистраций за текущий месяц
    Task<int> GetMonthlyNewClientsCountAsync();

    // Последние N входов для живой ленты на ресепшене
    Task<List<RecentPassDto>> GetRecentPassesAsync(int count = 10);
}