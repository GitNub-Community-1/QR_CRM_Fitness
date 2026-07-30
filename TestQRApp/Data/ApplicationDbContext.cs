using Microsoft.EntityFrameworkCore;
using TestQRApp.Models;
using TestQRApp.Models.Entity_s;
using TestQRApp.Models.Enums;

namespace TestQRApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Trainer> Trainers { get; set; } = null!;
    public DbSet<SubscriptionType> SubscriptionTypes { get; set; } = null!;
    public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;
    public DbSet<Staff> Staffs { get; set; } = null!;
    public DbSet<AccessLog> AccessLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Ограничения и индексы
        modelBuilder.Entity<Staff>()
            .HasIndex(s => s.Login)
            .IsUnique();

        modelBuilder.Entity<Staff>()
            .Property(s => s.Role)
            .HasConversion<string>(); // По желанию: хранить Enum как текст в Postgres

        // 2. Сидирование данных (Seed Data)
        var adminGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var moderatorGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var staticDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        modelBuilder.Entity<Staff>().HasData(
            new Staff
            {
                Id = adminGuid,
                Login = "admin",
                PasswordHash = "AQAAAAIAAYagAAAAEJ5dMv3Q8/uUeSjRY1l4zGZpT6mXnUvW1Y2Z3Q4R5t6y7u8i9o==",
                FirstName = "Главный",
                LastName = "Администратор",
                Role = StaffRole.Admin,
                CreatedAt = staticDate
            },
            new Staff
            {
                Id = moderatorGuid,
                Login = "moderator",
                PasswordHash = "AQAAAAIAAYagAAAAIModMv3Q8/uUeSjRY1l4zGZpT6mXnUvW1Y2Z3Q4R5t6y7u8i9o==",
                FirstName = "Дежурный",
                LastName = "Модератор",
                Role = StaffRole.Moderator,
                CreatedAt = staticDate
            }
        );
    }
}