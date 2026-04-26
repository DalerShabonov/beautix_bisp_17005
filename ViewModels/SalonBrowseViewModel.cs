namespace beautix_bisp_17005.ViewModels
{
    public class SalonBrowseViewModel
    {
        public int SalonId { get; set; }
        public string SalonName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ServiceItemViewModel> Services { get; set; } = new();
    }

    public class ServiceItemViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int CreditsRequired { get; set; }
        public decimal Price { get; set; }
    }
}