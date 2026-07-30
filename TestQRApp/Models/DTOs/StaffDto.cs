using TestQRApp.Models.Enums;

namespace TestQRApp.Models.DTOs;

public record StaffDto(Guid Id, string FullName, string Login, StaffRole Role);
