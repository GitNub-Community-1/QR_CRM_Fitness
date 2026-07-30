using Microsoft.EntityFrameworkCore;
using TestQRApp.Data;
using TestQRApp.Models.Entity_s;
using TestQRApp.Models.DTOs;
using TestQRApp.Services.interfaces;

namespace TestQRApp.Services;

public class TrainerService(AppDbContext db) : ITrainerService
{
    // 1. Создание тренера
    public async Task<Guid> CreateTrainerAsync(CreateTrainerDto dto)
    {
        var trainerId = Guid.NewGuid();

        var newTrainer = new Trainer
        {
            Id = trainerId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Specialization = dto.Specialization
        };

        await db.Trainers.AddAsync(newTrainer);
        await db.SaveChangesAsync();

        return trainerId;
    }

    // 2. Получение тренера по ID
    public async Task<TrainerDto?> GetTrainerByIdAsync(Guid id)
    {
        return await db.Trainers
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TrainerDto
            (
                t.Id,
                $"{t.LastName} {t.FirstName}".Trim(),
                t.Specialization
            ))
            .FirstOrDefaultAsync(); // Вернет null, если не найдет
    }

    // 3. Получение всех тренеров
    public async Task<List<TrainerDto>> GetAllTrainersAsync()
    {
        return await db.Trainers
            .AsNoTracking()
            .Select(t => new TrainerDto
            (
                t.Id,
                $"{t.LastName} {t.FirstName}".Trim(),
                t.Specialization
            ))
            .ToListAsync();
    }

    // 4. Обновление данных тренера
    public async Task<bool> UpdateTrainerAsync(Guid id, CreateTrainerDto dto)
    {
        var trainer = await db.Trainers.FindAsync(id);
        if (trainer == null)
        {
            return false;
        }

        trainer.FirstName = dto.FirstName;
        trainer.LastName = dto.LastName;
        trainer.Specialization = dto.Specialization;

        await db.SaveChangesAsync();
        return true;
    }

    // 5. Удаление тренера
    public async Task<bool> DeleteTrainerAsync(Guid id)
    {
        var trainer = await db.Trainers.FindAsync(id);
        if (trainer == null)
        {
            return false;
        }

        db.Trainers.Remove(trainer);
        await db.SaveChangesAsync();

        return true;
    }
}