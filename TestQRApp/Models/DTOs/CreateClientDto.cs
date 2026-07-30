using TestQRApp.Models.Enums;

namespace TestQRApp.Models.DTOs;

public record CreateClientDto(
    string FirstName, 
    string LastName, 
    string? MiddleName, 
    int Age, 
    Gender Gender, 
    string Login, 
    string Password, 
    string PhoneNumber, 
    Guid? TrainerId);