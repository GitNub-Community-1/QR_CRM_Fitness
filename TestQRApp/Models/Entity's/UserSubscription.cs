using TestQRApp.Models.Enums;

namespace TestQRApp.Models.Entity_s;

public class UserSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    
    public int SubcriptionTypeId { get; set; }
    public SubscriptionType SubscriptionType { get; set; } = null!;
    
    public bool IsActive { get; set; } = true; // Активен ли сейчас?
    public int RemainingVisits { get; set; }  // Сколько визитов осталось   
    
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public int SubscriptionTypeId { get; set; }
}