using TestQRApp.Models.Entity_s;

namespace TestQRApp.Models;

public class AccessLog
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public DateTime ScanTime { get; set; }
    public bool IsSuccess { get; set; } // Был ли успешным проход
    public string Note { get; set; } = string.Empty; // Причина отказа или детали
    
    // Навигационное свойство (если настраивал связь)
    public Client? Client { get; set; }
}