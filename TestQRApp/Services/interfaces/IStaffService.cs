using TestQRApp.Models.DTOs;

namespace TestQRApp.Services.interfaces;

public interface IStaffService
{
    Task<Guid> CreateStaffAsync(CreateStaffDto dto);
    Task<List<StaffDto>> GetAllStaffAsync();
    Task<bool> DeleteStaffAsync(Guid id);
}