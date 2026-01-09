using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Components;
using StudentProgressTracker.Configuration;
using StudentProgressTracker.Data;
using StudentProgressTracker.Hubs;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// FORWARDED HEADERS (pro Railway/proxy)
// ========================================

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ========================================
// KONFIGURACE
// ========================================

// Načtení konfiguračních sekcí do strongly-typed objektů
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(AppSettings.SectionName));

// ========================================
// RAZOR COMPONENTS (BLAZOR SERVER)
// ========================================

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ========================================
// DATABÁZE - ENTITY FRAMEWORK CORE
// ========================================

// Podpora SQL Server pro produkci a SQLite pro lokální vývoj
// Detekce typu databáze podle connection stringu
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // SQLite: connection string obsahuje "Data Source=" a končí ".db"
    if (connectionString?.Contains("Data Source=") == true && connectionString.EndsWith(".db"))
        options.UseSqlite(connectionString);
    else
        options.UseSqlServer(connectionString);
});

// ========================================
// AUTENTIZACE - GOOGLE OAUTH
// ========================================

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/";
    options.LogoutPath = "/auth/logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);  // Persistence přihlášení
})
.AddGoogle(options =>
{
    // Přihlašovací údaje z konfigurace (User Secrets v dev, env vars v prod)
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;

    // Handler volaný po úspěšné autentizaci u Google
    options.Events.OnCreatingTicket = async context =>
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
        var googleId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;

        if (!string.IsNullOrEmpty(email))
        {
            // Automatická registrace nových uživatelů jako učitelů
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    GoogleId = googleId,
                    DisplayName = name ?? email,
                    Role = UserRole.Teacher  // Noví uživatelé jsou automaticky učitelé
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            // Přidání vlastních claims pro RBAC
            var identity = (ClaimsIdentity?)context.Principal?.Identity;
            identity?.AddClaim(new Claim("UserId", user.Id));
            identity?.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();  // Pro Blazor komponenty

// ========================================
// SIGNALR - REAL-TIME KOMUNIKACE
// ========================================

builder.Services.AddSignalR();

// ========================================
// APLIKAČNÍ SLUŽBY
// ========================================

// HTTP klient pro Gemini API (typed HttpClient)
builder.Services.AddHttpClient<IAIService, GeminiAIService>();

// Scoped služby - nová instance per request
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IProgressHubService, ProgressHubService>();

// Singleton služby - jedna instance pro celou aplikaci
builder.Services.AddSingleton<IMarkdownService, MarkdownService>();

// ========================================
// RADZEN BLAZOR KOMPONENTY
// ========================================

builder.Services.AddScoped<Radzen.DialogService>();
builder.Services.AddScoped<Radzen.NotificationService>();
builder.Services.AddScoped<Radzen.TooltipService>();
builder.Services.AddScoped<Radzen.ContextMenuService>();

// ========================================
// SESTAVENÍ APLIKACE
// ========================================

var app = builder.Build();

// ========================================
// MIDDLEWARE PIPELINE
// ========================================

// Forwarded Headers musí být první - pro správné HTTPS za proxy (Railway)
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Autentizace a autorizace middleware
app.UseAuthentication();
app.UseAuthorization();

// ========================================
// ENDPOINTS
// ========================================

// SignalR Hub pro real-time komunikaci
app.MapHub<ProgressHub>("/hubs/progress");

// Endpoint pro spuštění Google OAuth přihlášení
app.MapGet("/auth/google-login", async (HttpContext context, string? returnUrl) =>
{
    var props = new AuthenticationProperties
    {
        RedirectUri = returnUrl ?? "/teacher/dashboard",
        IsPersistent = true
    };
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props);
});

// Endpoint pro odhlášení
app.MapGet("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

// Debug endpoint pro ověření funkčnosti routingu
app.MapGet("/debug/routes", () => "Routes are working!");

// Mapování Blazor komponent
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ========================================
// INICIALIZACE DATABÁZE
// ========================================

// Automatické vytvoření databáze při startu (EnsureCreated)
// Pro produkci použijte migrace místo EnsureCreated
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// ========================================
// SPUŠTĚNÍ APLIKACE
// ========================================

app.Run();
