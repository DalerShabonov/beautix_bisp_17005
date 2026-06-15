using beautix_bisp_17005.Models.Entities;

namespace beautix_bisp_17005.Services.Interfaces

{
    /// <summary>
    /// Contract for subscription and credit handling. Both the controllers and the
    /// BookingService depend on this abstraction rather than the concrete class.
    /// </summary>
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPlan>> GetAllPlansAsync();
        Task<UserSubscription?> GetActiveSubscriptionAsync(string userId);
        Task<bool> ActivateSubscriptionAsync(string userId, int planId);
        Task<bool> HasSufficientCreditsAsync(string userId, int creditsRequired);
        Task<bool> DeductCreditAsync(string userId, int creditsRequired);
        Task<bool> ReinstateCreditAsync(string userId, int creditsRequired);
    }
}