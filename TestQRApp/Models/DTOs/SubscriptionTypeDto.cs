namespace TestQRApp.Models.DTOs;

public record SubscriptionTypeDto(int Id, string Name, int DurationInDays, decimal Price, bool IsUnlimited);
