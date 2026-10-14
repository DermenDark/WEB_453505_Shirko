using LRs;
using LRs.ApiService;
using LRs.Components;
using LRs.Extensions;
using LRs.HelperClasses;
using LRs.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

var uriData = builder.Configuration
    .GetSection("UriData")
    .Get<UriData>();

if (uriData is null || string.IsNullOrWhiteSpace(uriData.ApiUri))
{
    throw new InvalidOperationException(
        "Не найден параметр UriData:ApiUri в appsettings.json");
}

var keycloakData = builder.Configuration
    .GetSection("KeycloakData")
    .Get<KeycloakData>();

if (keycloakData is null ||
    string.IsNullOrWhiteSpace(keycloakData.Host) ||
    string.IsNullOrWhiteSpace(keycloakData.Realm) ||
    string.IsNullOrWhiteSpace(keycloakData.ClientId) ||
    string.IsNullOrWhiteSpace(keycloakData.ClientSecret))
{
    throw new InvalidOperationException(
        "Проверьте настройки KeycloakData в appsettings.json");
}

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Bikes");
});

builder.Services.AddScoped<IRepository, CartRepository>();


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme = "keycloak";
    })
    .AddCookie()
    .AddOpenIdConnect("keycloak", options =>
    {
        var authority =
            $"{keycloakData.Host.TrimEnd('/')}/realms/{keycloakData.Realm}";

        options.Authority = authority;

        options.MetadataAddress =
            $"{authority}/.well-known/openid-configuration";

        options.ClientId = keycloakData.ClientId;
        options.ClientSecret = keycloakData.ClientSecret;

        options.ResponseType = OpenIdConnectResponseType.Code;

        options.Scope.Add("openid");
        options.SaveTokens = true;
        options.RequireHttpsMetadata = false;
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy =>
        policy.RequireRole("POWER-USER"));
});

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<KeycloakData>(
    builder.Configuration.GetSection("KeycloakData"));

builder.Services.AddHttpClient<ITokenAccessor, KeycloakTokenAccessor>();

var apiBaseUri = uriData.ApiUri.TrimEnd('/') + "/";

builder.Services.AddHttpClient<IProductService, ApiProductService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUri + "Bike/");
});

builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUri + "categories/");
});

builder.RegisterCustomServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .RequireAuthorization("admin");

app.Run();