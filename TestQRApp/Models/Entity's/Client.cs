using System.ComponentModel.DataAnnotations;
using TestQRApp.Models.Enums;

namespace TestQRApp.Models.Entity_s;

public class Client
{
    public Guid Id { get; set; }
    
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    public string? MiddleName { get; set; } 
    public int Age { get; set; }
    public Gender Gender { get; set; }
    
    // ---- ДОБАВЛЯЕМ СЮДА НОМЕР ТЕЛЕФОНА ----
    [Required]
    public string PhoneNumber { get; set; } = string.Empty; 
    // ----------------------------------------

    public string Login { get; set; }
    public string PasswordHash { get; set; }
    
    public string? ActiveDeviceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public Guid? TrainerId { get; set; }
    public Trainer? Trainer { get; set; }

    public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
    public ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
}