using beautix_bisp_17005.Data;
using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace beautix_bisp_17005.Services

{
    /// <summary>
    /// Handles everything to do with subscriptions and the "credit" system that
    /// gates bookings: listing plans, activating a plan, and checking/deducting/
    /// reinstating a subscriber's monthly service credits.
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        // EF Core context injected by the DI container (registered in Program.cs).
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Returns the public catalogue of plans, cheapest first, for the pricing page.
        public async Task<List<SubscriptionPlan>> GetAllPlansAsync()
        {
            return await _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.MonthlyPrice)
                .ToListAsync();
        }

        // Gets the user's single active subscription (or null). We Include the Plan
        // so callers can read the allowance/price without a second query.
        public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
        {
            return await _context.UserSubscriptions
                .Include(us => us.Plan)
                .FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive);
        }

        // Subscribes a user to a plan. If they already have one, the old
        // subscription is deactivated first so only ever one is active at a time.
        public async Task<bool> ActivateSubscriptionAsync(string userId, int planId)
        {
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive);

            if (plan == null)
                return false;

            var existing = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive);

            // Deactivate any current subscription before creating the new one.
            if (existing != null)
            {
                existing.IsActive = false;
                _context.UserSubscriptions.Update(existing);
            }

            // Fresh subscription: starts now, renews in a month, zero credits used.
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

        // Read-only check: does the user have enough remaining credits this month?
        // Unlimited plans (allowance 999) always pass. Used to gate the booking UI
        // before we attempt to actually deduct anything.
        public async Task<bool> HasSufficientCreditsAsync(string userId, int creditsRequired)
        {
            var subscription = await GetActiveSubscriptionAsync(userId);

            if (subscription == null)
                return false;

            // Unlimited tier — no need to count.
            if (subscription.Plan.ServiceAllowance == 999)
                return true;

            // Remaining = allowance - used; must cover what this booking costs.
            return (subscription.Plan.ServiceAllowance - subscription.ServicesUsed) >= creditsRequired;
        }

        // Spends credits when a booking is confirmed. Re-checks the balance here
        // (not just in HasSufficientCreditsAsync) so the actual deduction is safe
        // on its own. Unlimited plans skip the counter entirely.
        public async Task<bool> DeductCreditAsync(string userId, int creditsRequired)
        {
            var subscription = await GetActiveSubscriptionAsync(userId);

            if (subscription == null)
                return false;

            if (subscription.Plan.ServiceAllowance != 999)
            {
                // Guard against over-spending in case state changed since the check.
                if ((subscription.Plan.ServiceAllowance - subscription.ServicesUsed) < creditsRequired)
                    return false;

                subscription.ServicesUsed += creditsRequired;
                _context.UserSubscriptions.Update(subscription);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        // Gives credits back when a booking is cancelled. Math.Max(0, ...) makes
        // sure the used count can never go negative. Unlimited plans do nothing.
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