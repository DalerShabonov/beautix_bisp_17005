using beautix_bisp_17005.Data;
using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace beautix_bisp_17005.Services

{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPlan>> GetAllPlansAsync()
        {
            return await _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.MonthlyPrice)
                .ToListAsync();
        }

        public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
        {
            return await _context.UserSubscriptions
                .Include(us => us.Plan)
                .FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive);
        }

        public async Task<bool> ActivateSubscriptionAsync(string userId, int planId)
        {
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive);

            if (plan == null)
                return false;

            var existing = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive);

            if (existing != null)
            {
                existing.IsActive = false;
                _context.UserSubscriptions.Update(existing);
            }

            var subscription = new UserSubscription
            {
                UserId = userId,
                PlanId = planId,
                StartDate = DateTime.UtcNow,
                RenewalDate = DateTime.UtcNow.AddMonths(1),
                ServicesUsed = 0,
                IsActive = true,
                PaymentStatus = "Simulated"
            };

            await _context.UserSubscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasSufficientCreditsAsync(string userId, int creditsRequired)
        {
            var subscription = await GetActiveSubscriptionAsync(userId);

            if (subscription == null)
                return false;

            if (subscription.Plan.ServiceAllowance == 999)
                return true;

            return (subscription.Plan.ServiceAllowance - subscription.ServicesUsed) >= creditsRequired;
        }

        public async Task<bool> DeductCreditAsync(string userId, int creditsRequired)
        {
            var subscription = await GetActiveSubscriptionAsync(userId);

            if (subscription == null)
                return false;

            if (subscription.Plan.ServiceAllowance != 999)
            {
                if ((subscription.Plan.ServiceAllowance - subscription.ServicesUsed) < creditsRequired)
                    return false;

                subscription.ServicesUsed += creditsRequired;
                _context.UserSubscriptions.Update(subscription);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> ReinstateCreditAsync(string userId, int creditsRequired)
        {
            var subscription = await GetActiveSubscriptionAsync(userId);

            if (subscription == null)
                return false;

            if (subscription.Plan.ServiceAllowance != 999)
            {
                subscription.ServicesUsed = Math.Max(0, subscription.ServicesUsed - creditsRequired);
                _context.UserSubscriptions.Update(subscription);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}