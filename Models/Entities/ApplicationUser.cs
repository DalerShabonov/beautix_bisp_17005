using Microsoft.AspNetCore.Identity;

namespace beautix_bisp_17005.Models.Entities
{
    /// <summary>
    /// Represents an account in the system. We inherit from ASP.NET Identity's
    /// IdentityUser, so all the security plumbing (password hash, email, lockout,
    /// security stamp, etc.) comes for free. We only add the few profile fields
    /// Beautix actually needs on top of that.
    /// One ApplicationUser can be a Subscriber, a SalonPartner, or an Admin — the
    /// distinction is handled by Identity roles, not by separate tables.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // The customer's display name (Identity's UserName is the email, so we keep
        // the human-friendly name separately).
        public string FullName { get; set; } = string.Empty;

        // Nullable because a phone number is optional at registration.
        public string? PhoneNumber { get; set; }

        // Stored in UTC so date comparisons behave the same on any server/time zone.
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Navigation properties: EF Core uses these to load related rows.
        // A user can hold a history of subscriptions and many bookings.
        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
