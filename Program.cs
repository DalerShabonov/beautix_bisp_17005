using beautix_bisp_17005.Data;
using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services;
using beautix_bisp_17005.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Read SUPABASE_CONNECTION environment variable if present (Railway + Supabase)
var supabaseConnection = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION");

// Always use PostgreSQL — Npgsql for both local and production
// For local development set SUPABASE_CONNECTION in your environment
// For production Railway sets SUPABASE_CONNECTION automatically
if (!string.IsNullOrEmpty(supabaseConnection))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(supabaseConnection));
}
else
{
    // Fallback: read from appsettings.json
    // For local dev point DefaultConnection to your Supabase connection string
    var localConnection = builder.Configuration
        .GetConnectionString("DefaultConnection")!;

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(localConnection));
}

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

    // Auto-apply migrations on startup
    var db = services.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    await SeedRolesAsync(services);
    await SeedAdminUserAsync(services);
}

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

static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
{
    var adminService = serviceProvider
        .GetRequiredService<IAdminService>();

    await adminService.SeedAdminAsync(
        email: "admin@beautix.uz",
        password: "Admin@12345");
}