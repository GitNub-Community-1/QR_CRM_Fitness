using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TestQRApp.Components;
using TestQRApp.Services;
using TestQRApp.Services.interfaces;
using Microsoft.OpenApi.Models;
using TestQRApp.Data;

var builder = WebApplication.CreateBuilder(args);

// --- СИСТЕМНЫЕ СЕРВИСЫ ДЛЯ API И BLAZOR ---
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// 1. Подключение PostgreSQL через EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Настройка Cookie-авторизации
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FitPass.Auth";
        options.Cookie.HttpOnly = true; 
        
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; 
        options.Cookie.SameSite = SameSiteMode.Lax; 
        
        options.LoginPath = "/";
        options.AccessDeniedPath = "/";
        options.LogoutPath = "/api/auth/logout";
    });

builder.Services.AddAuthorization();

// Пробрасывает состояние авторизации в Blazor компоненты (.NET 8+)
builder.Services.AddCascadingAuthenticationState();

// 3. Регистрация бизнес-сервисов
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<IAnalytickcService, AnalytickcService>();
builder.Services.AddScoped<ITrainerService, TrainerService>();
builder.Services.AddScoped<ISubscriptionTypeService, SubscriptionTypeService>();
builder.Services.AddScoped<IClientManagerService, ClientManagerService>();
builder.Services.AddScoped<IStaffService, StaffService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 4. Настройка Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FitPass API", Version = "v1" });

    options.AddSecurityDefinition("CookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = "FitPass.Auth",
        Description = "Авторизация через сессионные куки."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CookieAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// ПОРЯДОК MIDDLEWARE КРИТИЧЕН:
app.UseAuthentication(); 
app.UseAuthorization();  

app.UseAntiforgery();

app.MapStaticAssets();

// Маппим эндпоинты наших API-контроллеров
app.MapControllers(); 

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ИНИЦИАЛИЗАЦИЯ И СИДДИНГ БАЗЫ ДАННЫХ (Один общий скоуп перед запуском приложения)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // Автоматически применяем миграции
        await context.Database.MigrateAsync();
        
        // Автоматически сбрасываем / создаем пользователей admin и moderator
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации/сиддинге базы данных.");
    }
}

app.Run();