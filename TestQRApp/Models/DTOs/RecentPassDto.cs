namespace TestQRApp.Models.DTOs;

// Расширяем до 6 параметров, которые запрашивает наша живая лента
public record RecentPassDto(
    Guid LogId, 
    Guid ClientId, 
    string ClientName, 
    DateTime ScanTime, 
    bool IsSuccess, 
    string Note
);