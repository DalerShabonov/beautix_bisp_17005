namespace beautix_bisp_17005.ViewModels
{
    public class SubscriptionPlanViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public int ServiceAllowance { get; set; }
        public bool IsCurrentPlan { get; set; }
    }
}