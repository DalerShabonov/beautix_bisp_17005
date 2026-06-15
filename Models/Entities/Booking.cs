namespace beautix_bisp_17005.Models.Entities
{
    /// <summary>
    /// The lifecycle states a booking can be in. Using an enum (instead of a
    /// loose string) means the compiler guarantees only valid values are used.
    /// </summary>
    public enum BookingStatus
    {
        Confirmed,   // Booked and active — this is the default when created.
        Cancelled,   // Cancelled by the customer (their credit is given back).
        Completed    // The appointment has happened.
    }

    /// <summary>
    /// A single appointment a subscriber makes for one service at a salon.
    /// This is the join between a customer (User) and a Service at a point in time.
    /// </summary>
    public class Booking
    {
        public int Id { get; set; }

        // Foreign key + navigation to the customer who made the booking.
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Nullable on purpose: if a salon later deletes a service, we keep the
        // booking record (for history) but the link becomes null rather than
        // deleting the booking. See the SetNull rule in ApplicationDbContext.
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }

        // When the appointment is scheduled for (stored in UTC).
        public DateTime AppointmentDate { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        // Audit field: when the row was created (UTC).
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
