using Microsoft.EntityFrameworkCore;
using TestQRApp.Data;
using TestQRApp.Models.Entity_s; 
using TestQRApp.Models.DTOs;
using TestQRApp.Models.Enums; 
using TestQRApp.Services.interfaces;

namespace TestQRApp.Services;

public class StaffService(AppDbContext db) : IStaffService
{
    // 1. Создание сотрудника
    public async Task<Guid> CreateStaffAsync(CreateStaffDto dto)
    {
        var isDuplicate = await db.Staffs.AnyAsync(s => s.Login == dto.Login);
        if (isDuplicate)
        {
            throw new InvalidOperationException("Сотрудник с таким логином уже зарегистрирован.");
        }

        var staffId = Guid.NewGuid();

        var newStaff = new Staff
        {
            Id = staffId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Login = dto.Login,
            // Просто присваиваем напрямую, так как dto.Role уже имеет тип StaffRole!
            Role = dto.Role, 
            CreatedAt = DateTimeOffset.UtcNow
        };

        await db.Staffs.AddAsync(newStaff);
        await db.SaveChangesAsync();

        return staffId;
    }

    // 2. Получение всех сотрудников
    public async Task<List<StaffDto>> GetAllStaffAsync()
    {
        return await db.Staffs
            .AsNoTracking() // Отключаем отслеживание для ускорения работы с PostgreSQL
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StaffDto
            (
                s.Id,
                $"{s.LastName} {s.FirstName}".Trim(),
                s.Login,
                s.Role // Передаем напрямую перечисление
            ))
            .ToListAsync(); 
    }

    // 3. Удаление сотрудника
    public async Task<bool> DeleteStaffAsync(Guid id)
    {
        var staff = await db.Staffs.FindAsync(id);
        
        if (staff == null)
        {
            return false;
        }

        db.Staffs.Remove(staff);
        await db.SaveChangesAsync();

        return true;
    }
}