using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TestQRApp.Data;
using TestQRApp.Models.Entity_s; 
using TestQRApp.Models.DTOs;
using TestQRApp.Services.interfaces;

namespace TestQRApp.Services;

public class ClientManagerService(AppDbContext db) : IClientManagerService
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    // 1. Регистрация нового клиента модератором
    public async Task<Guid> RegisterClientAsync(CreateClientDto dto)
    {
        var exists = await db.Clients.AnyAsync(c => c.Login == dto.Login);
        if (exists)
        {
            throw new InvalidOperationException("Клиент с таким логином уже существует.");
        }

        var clientId = Guid.NewGuid();
        string passwordHash = _passwordHasher.HashPassword(null!, dto.Password);

        var newClient = new Client
        {
            Id = clientId,
            Login = dto.Login,
            PasswordHash = passwordHash,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            Age = dto.Age,
            Gender = dto.Gender,
            PhoneNumber = dto.PhoneNumber, 
            CreatedAt = DateTimeOffset.UtcNow
        };

        await db.Clients.AddAsync(newClient);
        await db.SaveChangesAsync();

        return clientId;
    }

    // 2. Привязка или продление абонемента
    // 2. Привязка или продление абонемента
    public async Task<bool> AssignSubscriptionAsync(Guid clientId, int subscriptionTypeId)
    {
        var client = await db.Clients.FindAsync(clientId);
        var subType = await db.SubscriptionTypes.FindAsync(subscriptionTypeId);

        if (client == null || subType == null)
        {
            return false;
        }

        var startDate = DateTimeOffset.UtcNow;
        var endDate = startDate.AddDays(subType.DurationInDate); 

        var activeSubs = await db.UserSubscriptions
            .Where(us => us.ClientId == clientId && us.IsActive)
            .ToListAsync();

        foreach (var sub in activeSubs)
        {
            sub.IsActive = false;
        }

        var userSubscription = new UserSubscription
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            SubscriptionTypeId = subscriptionTypeId, // ИСПРАВЛЕНО: добавлена пропущенная буква 's' (SubscriptionTypeId)
            StartDate = startDate,
            EndDate = endDate,
            RemainingVisits = subType.IsUnlimited ? 0 : 30, 
            IsActive = true
        };

        await db.UserSubscriptions.AddAsync(userSubscription);
        await db.SaveChangesAsync();

        return true;
    }
    // 3. Список клиентов с поиском и пагинацией
    public async Task<PagedListDto<ClientListDto>> GetClientsAsync(string searchTerm, int page, int pageSize)
    {
        var query = db.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c => 
                c.FirstName.ToLower().Contains(term) || 
                c.LastName.ToLower().Contains(term) || 
                c.PhoneNumber.Contains(term) || 
                c.Login.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClientListDto
            (
                c.Id,
                $"{c.LastName} {c.FirstName}".Trim(),
                c.PhoneNumber,
                c.Login, // Передаем 4-й параметр (Login)
                true     // Передаем 5-й параметр (IsActive)
            ))
            .ToListAsync();

        return new PagedListDto<ClientListDto>(items, totalCount, page, pageSize);
    }

    // 4. Детальная информация по клиенту
    public async Task<ClientDetailDto?> GetClientDetailAsync(Guid clientId)
    {
        var client = await db.Clients.FindAsync(clientId);
        if (client == null) return null;

        var today = DateTimeOffset.UtcNow;
        var activeSub = await db.UserSubscriptions
            .Include(us => us.SubscriptionType)
            .FirstOrDefaultAsync(us => 
                us.ClientId == clientId && 
                us.StartDate <= today && 
                us.EndDate >= today && 
                us.IsActive);

        // Передаем ровно 10 параметров в конструктор ClientDetailDto
        return new ClientDetailDto(
            client.Id,
            client.FirstName,
            client.LastName,
            client.PhoneNumber,
            client.Login,
            client.CreatedAt.DateTime,
            client.ActiveDeviceId,
            activeSub?.SubscriptionType.Name,
            activeSub?.SubscriptionType.IsUnlimited == true ? "Безлимит" : activeSub?.RemainingVisits.ToString(),
            activeSub?.EndDate.DateTime
        );
    }
}