namespace TestQRApp.Models.Entity_s;

public class SubscriptionType
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DurationInDate { get; set; }
    public decimal Price { get; set; }
    public bool IsUnlimited { get; set; } // Безлимитный ли абонемент?
    public ICollection<UserSubscription> UserSubscriptions = new List<UserSubscription>();
}