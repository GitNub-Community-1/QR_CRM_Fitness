namespace TestQRApp.Models.DTOs;

public record ClientListDto(
    Guid Id, 
    string FullName, 
    string PhoneNumber, 
    string Login, // <-- Добавили 5-й параметр
    bool IsActive
);