using TestQRApp.Models.DTOs;

namespace TestQRApp.Services.interfaces;

public interface IClientManagerService
{
    // Регистрация клиента модератором
    Task<Guid> RegisterClientAsync(CreateClientDto dto);

    // Привязка/продление абонемента
    Task<bool> AssignSubscriptionAsync(Guid clientId, int subscriptionTypeId);

    // Список клиентов с поиском и пагинацией для административной панели
    Task<PagedListDto<ClientListDto>> GetClientsAsync(string searchTerm, int page, int pageSize);

    // Детальная информация по клиенту
    Task<ClientDetailDto?> GetClientDetailAsync(Guid clientId);
}