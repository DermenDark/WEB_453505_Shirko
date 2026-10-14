using System.Configuration;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using WEB_453505_Shirko.API;
using Microsoft.IdentityModel.Tokens;
using WEB_453505_Shirko.API.Data;
using WEB_453505_Shirko.API.Extensions;
using WEB_453505_Shirko.API.Models;
using WEB_453505_Shirko.API.UseCases;

var builder = WebApplication.CreateBuilder(args);
var authServer = builder.Configuration
    .GetSection("AuthServer")
    .Get<AuthServerData>()
    ?? throw new InvalidOperationException(
        "Не найдены настройки AuthServer.");

if (string.IsNullOrWhiteSpace(authServer.Host) ||
    string.IsNullOrWhiteSpace(authServer.Realm))
{
    throw new InvalidOperationException(
        "Проверь настройки AuthServer:Host и AuthServer:Realm.");
}

var realmUrl =
    $"{authServer.Host.TrimEnd('/')}/realms/{authServer.Realm}";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = realmUrl;

        options.MetadataAddress =
            $"{realmUrl}/.well-known/openid-configuration";

        options.Audience = "account";

        // Для локального Keycloak по HTTP.
        options.RequireHttpsMetadata = false;

        // Роль из JWT будет читаться из claim "role".
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                RoleClaimType = "role"
            };
    });

builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(
            "admin",
            policy => policy.RequireRole("POWER-USER"));
    });

builder.Services.AddControllersWithViews();
var connectionString =
    builder.Configuration.GetConnectionString("Postgres");

var settings =
    new Npgsql.NpgsqlConnectionStringBuilder(connectionString);

Console.WriteLine(
    $"DB config: Host={settings.Host}; " +
    $"Port={settings.Port}; " +
    $"Database={settings.Database}; " +
    $"Username={settings.Username}; " +
    $"PasswordIs123456={settings.Password == "123456"}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetListOfProducts).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

await DbInitializer.SeedData(app);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapIEndpoints();

app.Run();