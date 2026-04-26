namespace beautix_bisp_17005.ViewModels
{
    public class SubscriptionDashboardViewModel
    {
        public bool HasActiveSubscription { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public int ServiceAllowance { get; set; }
        public int ServicesUsed { get; set; }
        public int ServicesRemaining => ServiceAllowance == 999
            ? int.MaxValue
            : ServiceAllowance - ServicesUsed;
        public bool IsUnlimited => ServiceAllowance == 999;
        public DateTime StartDate { get; set; }
        public DateTime RenewalDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}