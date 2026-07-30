using TestQRApp.Models.Enums;

namespace TestQRApp.Models.DTOs;

public record CreateStaffDto(string FirstName, string LastName, string Login, string Password, StaffRole Role);
