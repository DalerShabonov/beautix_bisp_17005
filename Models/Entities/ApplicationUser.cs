using Microsoft.AspNetCore.Identity;

namespace beautix_bisp_17005.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
