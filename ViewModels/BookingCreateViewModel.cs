using System.ComponentModel.DataAnnotations;

namespace beautix_bisp_17005.ViewModels
{
    public class BookingCreateViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string SalonName { get; set; } = string.Empty;
        public int CreditsRequired { get; set; }
        public int DurationMinutes { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Appointment Date and Time")]
        public DateTime AppointmentDate { get; set; } = DateTime.UtcNow.AddDays(1);
    }
}