using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.ViewModels;

namespace beautix_bisp_17005.Services.Interfaces
{
    public interface IBookingService
    {
        Task<List<SalonBrowseViewModel>> GetApprovedSalonsWithServicesAsync();
        Task<BookingCreateViewModel?> GetBookingFormAsync(int serviceId);
        Task<(bool Success, string Message)> CreateBookingAsync(string userId, int serviceId, DateTime appointmentDate);
        Task<List<BookingListViewModel>> GetUserBookingsAsync(string userId);
        Task<(bool Success, string Message)> CancelBookingAsync(string userId, int bookingId);
    }
}