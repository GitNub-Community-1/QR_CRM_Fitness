using Microsoft.EntityFrameworkCore;
using TestQRApp.Data;
using TestQRApp.Models.DTOs;
using TestQRApp.Models.Entity_s;
using TestQRApp.Services.interfaces;

namespace TestQRApp.Services;

public class SubscriptionTypeService(AppDbContext db) : ISubscriptionTypeService
{
    public async Task<int> CreateTypeAsync(CreateSubscriptionTypeDto dto)
    {
        var newSub = new SubscriptionType
        {
            Name =  dto.Name,
            DurationInDate = dto.DurationInDays,
            Price = dto.Price,
            IsUnlimited = dto.IsUnlimited
        };
        await db.SubscriptionTypes.AddAsync(newSub);
        await db.SaveChangesAsync();
        return newSub.Id;
    }

    public async Task<List<SubscriptionTypeDto>> GetAllTypesAsync()
    {
        return await db.SubscriptionTypes
            .AsNoTracking() // Отключаем трекинг для скорости 🚀
            .Select(st => new SubscriptionTypeDto
            (
                st.Id,
                st.Name,
                st.DurationInDate, // Или DurationInDays (как у тебя в DTO)
                st.Price,
                st.IsUnlimited
            ))
            .ToListAsync(); // Теперь это список SubscriptionTypeDto, компилятор счастлив!
    }

    public async Task<bool> UpdateTypeAsync(int id, CreateSubscriptionTypeDto dto)
    {
        var sub = await db.SubscriptionTypes.FindAsync(id);
        if (sub == null)
        {
            return false;
        }
        
        sub.Name = dto.Name;
        sub.DurationInDate = dto.DurationInDays;
        sub.Price = dto.Price;
        sub.IsUnlimited = dto.IsUnlimited;
        
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTypeAsync(int id)
    {
        var sub = await db.SubscriptionTypes.FindAsync(id);
        
        if (sub == null)
        {
            return false;
        }

        db.SubscriptionTypes.Remove(sub);
        await db.SaveChangesAsync();

        return true;
    }
}