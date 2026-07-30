using TestQRApp.Models.DTOs;

namespace TestQRApp.Services.interfaces;

public interface ITrainerService
{
    Task<Guid> CreateTrainerAsync(CreateTrainerDto dto);
    Task<TrainerDto?> GetTrainerByIdAsync(Guid id);
    Task<List<TrainerDto>> GetAllTrainersAsync();
    Task<bool> UpdateTrainerAsync(Guid id, CreateTrainerDto dto);
    Task<bool> DeleteTrainerAsync(Guid id);
}