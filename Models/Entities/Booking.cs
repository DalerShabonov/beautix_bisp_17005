namespace beautix_bisp_17005.Models.Entities
{
    public enum BookingStatus
    {
        Confirmed,
        Cancelled,
        Completed
    }

    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int? ServiceId { get; set; }         
        public Service? Service { get; set; }
        public DateTime AppointmentDate { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
