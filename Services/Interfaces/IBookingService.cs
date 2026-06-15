using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.ViewModels;

namespace beautix_bisp_17005.Services.Interfaces
{
    /// <summary>
    /// Contract for the booking workflow. Controllers depend on this interface
    /// (not the concrete class), which keeps them loosely coupled and makes the
    /// service easy to mock/swap. Wired up in Program.cs via AddScoped.
    /// </summary>
    public interface IBookingService
    {
        Task<List<SalonBrowseViewModel>> GetApprovedSalonsWithServicesAsync();
        Task<BookingCreateViewModel?> GetBookingFormAsync(int serviceId);
        Task<(bool Success, string Message)> CreateBookingAsync(string userId, int serviceId, DateTime appointmentDate);
        Task<List<BookingListViewModel>> GetUserBookingsAsync(string userId);
        Task<(bool Success, string Message)> CancelBookingAsync(string userId, int bookingId);
    }
}