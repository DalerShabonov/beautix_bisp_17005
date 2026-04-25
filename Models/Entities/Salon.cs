namespace beautix_bisp_17005.Models.Entities
{
    public class Salon
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
        public string OwnerUserId { get; set; } = string.Empty;  
        public ApplicationUser Owner { get; set; } = null!;       
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
