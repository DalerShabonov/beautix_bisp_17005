namespace beautix_bisp_17005.Models.Entities
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int CreditsRequired { get; set; } = 1;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int SalonId { get; set; }
        public Salon Salon { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
