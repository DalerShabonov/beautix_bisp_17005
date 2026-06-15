namespace beautix_bisp_17005.Models.Entities
{
    /// <summary>
    /// A single bookable treatment offered by a salon (e.g. "Haircut", "Manicure").
    /// Each service belongs to exactly one salon and costs a number of subscription
    /// "credits" to book, rather than being paid for directly.
    /// </summary>
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }

        // How many of the subscriber's monthly credits this service uses up.
        // Defaults to 1 so a simple service costs one credit.
        public int CreditsRequired { get; set; } = 1;

        // The cash price (informational/display). Stored as decimal for money
        // accuracy — mapped to numeric(18,2) in the DbContext.
        public decimal Price { get; set; }

        // Lets a salon temporarily hide a service without deleting it.
        public bool IsAvailable { get; set; } = true;

        // Foreign key + navigation back to the owning salon.
        public int SalonId { get; set; }
        public Salon Salon { get; set; } = null!;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
