using TestQRApp.Models.DTOs;

namespace TestQRApp.Services.interfaces;

public interface IAccessControlService
{
    // Здесь должен быть ТОЛЬКО метод обработки входа!
    Task<AccessResultDto> ProcessEntryAsync(string qrToken);
}