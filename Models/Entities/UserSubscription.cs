namespace beautix_bisp_17005.Models.Entities
{
    public class UserSubscription
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime RenewalDate { get; set; }
        public int ServicesUsed { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string PaymentStatus { get; set; } = "Simulated";
    }
}
