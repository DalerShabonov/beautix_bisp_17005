namespace beautix_bisp_17005.ViewModels
{
    public class SalonDashboardViewModel
    {
        public int SalonId { get; set; }
        public string SalonName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int TodaysBookings { get; set; }
        public List<SalonBookingItemViewModel> RecentBookings { get; set; } = new();
        public List<SalonServiceViewModel> Services { get; set; } = new();
    }

    public class SalonBookingItemViewModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SalonServiceViewModel
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int CreditsRequired { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}