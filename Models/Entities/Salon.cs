namespace beautix_bisp_17005.Models.Entities
{
    /// <summary>
    /// A beauty salon registered on the platform by a SalonPartner. A salon is
    /// only visible to customers once an admin approves it (IsApproved), which
    /// gives the platform a moderation/quality gate before salons go live.
    /// </summary>
    public class Salon
    {
        public int Id { get; set; }

        // Basic profile / contact details shown to customers.
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;   // Used so customers can browse by area.
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Moderation flag. New salons start as false and stay hidden from the
        // booking pages until an admin approves them.
        public bool IsApproved { get; set; } = false;

        // Foreign key + navigation to the SalonPartner who owns this salon.
        // We use this to make sure a partner can only edit their own salon.
        public string OwnerUserId { get; set; } = string.Empty;
        public ApplicationUser Owner { get; set; } = null!;

        // The list of services this salon offers (one-to-many).
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
