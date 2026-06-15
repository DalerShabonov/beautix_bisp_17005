namespace beautix_bisp_17005.Models.Entities
{
    /// <summary>
    /// A subscription tier the platform offers (Basic / Standard / Premium).
    /// This is the *catalogue* of plans — the actual link between a customer and a
    /// plan they bought lives in UserSubscription. The three default plans are
    /// seeded in ApplicationDbContext.OnModelCreating.
    /// </summary>
    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Monthly cost — decimal for money accuracy (numeric(18,2) in the DB).
        public decimal MonthlyPrice { get; set; }

        // How many service credits the plan grants each month.
        // Note the convention: 999 is treated as "unlimited" throughout the
        // SubscriptionService logic.
        public int ServiceAllowance { get; set; }

        // Lets us retire a plan without deleting it (kept for existing subscribers).
        public bool IsActive { get; set; } = true;

        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
