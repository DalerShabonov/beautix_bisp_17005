using beautix_bisp_17005.Data;
using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services.Interfaces;
using beautix_bisp_17005.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace beautix_bisp_17005.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var allUsers = await _userManager.Users.ToListAsync();

            var subscribers = new List<ApplicationUser>();
            var salonPartners = new List<ApplicationUser>();

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Subscriber"))
                    subscribers.Add(user);
                if (await _userManager.IsInRoleAsync(user, "SalonPartner"))
                    salonPartners.Add(user);
            }

            var salons = await _context.Salons.ToListAsync();
            var bookings = await _context.Bookings.ToListAsync();
            var subscriptions = await _context.UserSubscriptions
                .Include(us => us.Plan)
                .Where(us => us.IsActive)
                .ToListAsync();

            var revenue = subscriptions.Sum(us => us.Plan.MonthlyPrice);

            var recentUsers = allUsers
                .OrderByDescending(u => u.RegisteredAt)
                .Take(8)
                .ToList();

            var recentUserViewModels = new List<AdminUserViewModel>();
            foreach (var user in recentUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var hasSubscription = await _context.UserSubscriptions
                    .AnyAsync(us => us.UserId == user.Id && us.IsActive);

                recentUserViewModels.Add(new AdminUserViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "Unknown",
                    RegisteredAt = user.RegisteredAt,
                    HasActiveSubscription = hasSubscription,
                    IsLockedOut = user.LockoutEnd.HasValue &&
                                  user.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            var pendingSalons = await _context.Salons
                .Include(s => s.Owner)
                .Include(s => s.Services)
                .Where(s => !s.IsApproved)
                .Select(s => new AdminSalonViewModel
                {
                    SalonId = s.Id,
                    SalonName = s.Name,
                    OwnerName = s.Owner.FullName,
                    OwnerEmail = s.Owner.Email ?? string.Empty,
                    District = s.District,
                    ContactPhone = s.ContactPhone,
                    IsApproved = s.IsApproved,
                    TotalServices = s.Services.Count,
                    TotalBookings = 0
                })
                .ToListAsync();

            return new AdminDashboardViewModel
            {
                TotalUsers = allUsers.Count,
                TotalSubscribers = subscribers.Count,
                TotalSalonPartners = salonPartners.Count,
                TotalSalons = salons.Count,
                PendingSalons = salons.Count(s => !s.IsApproved),
                ApprovedSalons = salons.Count(s => s.IsApproved),
                TotalBookings = bookings.Count,
                ConfirmedBookings = bookings.Count(b =>
                    b.Status == BookingStatus.Confirmed),
                CancelledBookings = bookings.Count(b =>
                    b.Status == BookingStatus.Cancelled),
                TotalActiveSubscriptions = subscriptions.Count,
                TotalSimulatedRevenue = revenue,
                RecentUsers = recentUserViewModels,
                PendingSalonList = pendingSalons
            };
        }

        public async Task<List<AdminUserViewModel>> GetAllUsersAsync()
        {
            var allUsers = await _userManager.Users
                .OrderByDescending(u => u.RegisteredAt)
                .ToListAsync();

            var result = new List<AdminUserViewModel>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var hasSubscription = await _context.UserSubscriptions
                    .AnyAsync(us => us.UserId == user.Id && us.IsActive);

                result.Add(new AdminUserViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "Unknown",
                    RegisteredAt = user.RegisteredAt,
                    HasActiveSubscription = hasSubscription,
                    IsLockedOut = user.LockoutEnd.HasValue &&
                                  user.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            return result;
        }

        public async Task<List<AdminSalonViewModel>> GetAllSalonsAsync()
        {
            return await _context.Salons
                .Include(s => s.Owner)
                .Include(s => s.Services)
                .Select(s => new AdminSalonViewModel
                {
                    SalonId = s.Id,
                    SalonName = s.Name,
                    OwnerName = s.Owner.FullName,
                    OwnerEmail = s.Owner.Email ?? string.Empty,
                    District = s.District,
                    ContactPhone = s.ContactPhone,
                    IsApproved = s.IsApproved,
                    TotalServices = s.Services.Count,
                    TotalBookings = s.Services
                        .SelectMany(sv => sv.Bookings).Count()
                })
                .OrderBy(s => s.IsApproved)
                .ThenBy(s => s.SalonName)
                .ToListAsync();
        }

        public async Task<bool> ApproveSalonAsync(int salonId)
        {
            var salon = await _context.Salons.FindAsync(salonId);
            if (salon == null) return false;

            salon.IsApproved = true;
            _context.Salons.Update(salon);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SuspendSalonAsync(int salonId)
        {
            var salon = await _context.Salons.FindAsync(salonId);
            if (salon == null) return false;

            salon.IsApproved = false;
            _context.Salons.Update(salon);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            await _userManager.SetLockoutEndDateAsync(
                user, DateTimeOffset.UtcNow.AddYears(100));
            return true;
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            await _userManager.SetLockoutEndDateAsync(user, null);
            return true;
        }

        public async Task<bool> SeedAdminAsync(string email, string password)
        {
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null) return false;

            var admin = new ApplicationUser
            {
                FullName = "Platform Administrator",
                Email = email,
                UserName = email,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(admin, password);
            if (!result.Succeeded) return false;

            await _userManager.AddToRoleAsync(admin, "Admin");
            return true;
        }
    }
}