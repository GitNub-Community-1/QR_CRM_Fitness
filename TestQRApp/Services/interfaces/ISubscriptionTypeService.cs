using TestQRApp.Models.DTOs;

namespace TestQRApp.Services.interfaces;

public interface ISubscriptionTypeService
{
    Task<int> CreateTypeAsync(CreateSubscriptionTypeDto dto);
    Task<List<SubscriptionTypeDto>> GetAllTypesAsync();
    Task<bool> UpdateTypeAsync(int id, CreateSubscriptionTypeDto dto);
    Task<bool> DeleteTypeAsync(int id);
}