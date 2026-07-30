namespace TestQRApp.Models.DTOs;

// Сделаем красивый C# 12 Primary Constructor из 3 полей:
public record AccessResultDto(bool Success, string Message, string? ClientName);