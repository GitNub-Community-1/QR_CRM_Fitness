using TestQRApp.Models.Enums;

namespace TestQRApp.Models.Entity_s;

public class Staff
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public StaffRole Role { get; set; }
}