namespace TestQRApp.Models.DTOs;

public record CreateSubscriptionTypeDto(string Name, int DurationInDays, decimal Price,  bool IsUnlimited);
