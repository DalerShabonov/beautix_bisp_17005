using beautix_bisp_17005.Data;
using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services;
using beautix_bisp_17005.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database connection string is resolved below (env var first, then appsettings).

// Production (Railway): set the SUPABASE_CONNECTION environment variable.
// Local dev: use `dotnet user-secrets` or ConnectionStrings:DefaultConnection.
// Never commit a real connection string to source control.
var connectionString =
    Environment.GetEnvironmentVariable("SUPABASE_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No database connection configured. Set the SUPABASE_CONNECTION environment variable " +
        "(Railway) or ConnectionStrings:DefaultConnection (local user-secrets / appsettings).");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("beautix_bisp_17005");

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ISalonService, SalonService>();
builder.Services.AddScoped<IAdminService, AdminService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        // Auto-apply migrations on startup
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        await SeedRolesAsync(services);
        await SeedAdminUserAsync(services, app.Logger);

        app.Logger.LogInformation("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        // A database outage must not take the whole site down with a blank page.
        // The app still boots so database-independent pages (e.g. the landing page)
        // keep serving; database-backed features stay unavailable until the
        // connection is restored.
        app.Logger.LogError(ex,
            "Database initialization failed at startup. The app will keep running, " +
            "but database-backed features are unavailable until the DB connection is restored.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

static async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Subscriber", "SalonPartner", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

static async Task SeedAdminUserAsync(IServiceProvider serviceProvider, ILogger logger)
{
    // Admin credentials come from environment variables so they are never
    // committed to source control. If either is missing, skip seeding.
    var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
    var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        logger.LogWarning(
            "ADMIN_EMAIL / ADMIN_PASSWORD not set - skipping admin user seeding. " +
            "Set both environment variables to create the platform administrator.");
        return;
    }

    var adminService = serviceProvider
        .GetRequiredService<IAdminService>();

    await adminService.SeedAdminAsync(email, password);
}