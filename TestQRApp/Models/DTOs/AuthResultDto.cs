using TestQRApp.Models.Enums;

namespace TestQRApp.Models.DTOs;

public record AuthResultDto(
    bool IsSuccess, 
    string ErrorMessage, 
    StaffRole? Role, 
    string? FirstName
    );
