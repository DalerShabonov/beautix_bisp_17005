namespace beautix_bisp_17005.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalSubscribers { get; set; }
        public int TotalSalonPartners { get; set; }
        public int TotalSalons { get; set; }
        public int PendingSalons { get; set; }
        public int ApprovedSalons { get; set; }
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int TotalActiveSubscriptions { get; set; }
        public decimal TotalSimulatedRevenue { get; set; }
        public List<AdminUserViewModel> RecentUsers { get; set; } = new();
        public List<AdminSalonViewModel> PendingSalonList { get; set; } = new();
    }

    public class AdminUserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public bool HasActiveSubscription { get; set; }
        public bool IsLockedOut { get; set; }
    }

    public class AdminSalonViewModel
    {
        public int SalonId { get; set; }
        public string SalonName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public int TotalServices { get; set; }
        public int TotalBookings { get; set; }
    }
}