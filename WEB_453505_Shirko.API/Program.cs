using System.Configuration;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using WEB_453505_Shirko.API;
using WEB_453505_Shirko.API.Data;
using WEB_453505_Shirko.API.Extensions;
using WEB_453505_Shirko.API.Models;
using WEB_453505_Shirko.API.UseCases;

var builder = WebApplication.CreateBuilder(args);
var authServer = builder.Configuration
    .GetSection("AuthServer")
    .Get<AuthServerData>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        // Адрес метаданных конфигурации OpenID 
        o.MetadataAddress = $"{authServer.Host}/realms/{authServer.Realm}/.well-known/openid-configuration"; 

        // Authority сервера аутентификации 
        o.Authority = $"{authServer.Host}/realms/{authServer.Realm}";

        // Audience для токена JWT 
        o.Audience = "account";

        // Запретить HTTPS для использования локальной версии Keycloak 
        // В рабочем проекте должно быть true 
        o.RequireHttpsMetadata = false;
    });
builder.Services.AddAuthorization(opt =>
    {
        opt.AddPolicy("admin", p => p.RequireRole("POWER-USER"));
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//app.MapStaticAssets() со сборки
app.UseStaticFiles();
// с диска читает
app.MapIEndpoints();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Run();