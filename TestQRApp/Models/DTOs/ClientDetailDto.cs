namespace TestQRApp.Models.DTOs;

public record ClientDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Login,
    DateTime CreatedAt,
    string? ActiveDeviceId,
    string? SubscriptionName,      // Имя абонемента
    string? RemainingVisitsStatus, // "Безлимит" или число визитов
    DateTime? SubscriptionEndDate  // Дата окончания
);