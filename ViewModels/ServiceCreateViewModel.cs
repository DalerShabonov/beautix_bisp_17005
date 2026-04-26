using System.ComponentModel.DataAnnotations;

namespace beautix_bisp_17005.ViewModels
{
    public class ServiceCreateViewModel
    {
        [Required]
        [Display(Name = "Service Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(10, 480)]
        [Display(Name = "Duration (minutes)")]
        public int DurationMinutes { get; set; } = 60;

        [Required]
        [Range(1, 5)]
        [Display(Name = "Credits Required")]
        public int CreditsRequired { get; set; } = 1;

        [Required]
        [Range(0.01, 10000)]
        [Display(Name = "Price ($)")]
        public decimal Price { get; set; }
    }
}