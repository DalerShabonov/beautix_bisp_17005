namespace beautix_bisp_17005.Models.Entities
{
    /// <summary>
    /// Links a customer to a SubscriptionPlan they have purchased — i.e. one
    /// person's active (or past) membership. This is where we track how many
    /// credits they've used this period and when it renews.
    /// </summary>
    public class UserSubscription
    {
        public int Id { get; set; }

        // Foreign key + navigation to the subscriber.
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Foreign key + navigation to the chosen plan.
        public int PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime RenewalDate { get; set; }   // StartDate + 1 month.

        // Credits consumed this billing period. Compared against the plan's
        // ServiceAllowance to decide whether a new booking is allowed.
        public int ServicesUsed { get; set; } = 0;

        // Only one subscription per user should be active at a time. Switching
        // plans deactivates the old row and creates a new active one.
        public bool IsActive { get; set; } = true;

        // No real payment gateway is wired up for this project, so payments are
        // recorded as "Simulated" to make that explicit.
        public string PaymentStatus { get; set; } = "Simulated";
    }
}
