using beautix_bisp_17005.ViewModels;

namespace beautix_bisp_17005.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();
        Task<List<AdminUserViewModel>> GetAllUsersAsync();
        Task<List<AdminSalonViewModel>> GetAllSalonsAsync();
        Task<bool> ApproveSalonAsync(int salonId);
        Task<bool> SuspendSalonAsync(int salonId);
        Task<bool> LockUserAsync(string userId);
        Task<bool> UnlockUserAsync(string userId);
        Task<bool> SeedAdminAsync(string email, string password);
    }
}