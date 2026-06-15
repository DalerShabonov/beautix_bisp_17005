using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.ViewModels;

namespace beautix_bisp_17005.Services.Interfaces
{
    /// <summary>
    /// Contract for salon and service management used by the SalonController.
    /// </summary>
    public interface ISalonService
    {
        Task<Salon?> GetSalonByOwnerAsync(string ownerId);
        Task<bool> CreateSalonAsync(string ownerId, SalonCreateViewModel model);
        Task<bool> UpdateSalonAsync(int salonId, string ownerId, SalonCreateViewModel model);
        Task<SalonDashboardViewModel?> GetDashboardAsync(string ownerId);
        Task<bool> AddServiceAsync(int salonId, string ownerId, ServiceCreateViewModel model);
        Task<bool> ToggleServiceAvailabilityAsync(int serviceId, string ownerId);
        Task<bool> DeleteServiceAsync(int serviceId, string ownerId);
    }
}