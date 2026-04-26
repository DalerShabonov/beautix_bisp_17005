using System.ComponentModel.DataAnnotations;

namespace beautix_bisp_17005.ViewModels
{
    public class SalonCreateViewModel
    {
        [Required]
        [Display(Name = "Salon Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Street Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "District")]
        public string District { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;

        [Display(Name = "Salon Description")]
        public string Description { get; set; } = string.Empty;
    }
}