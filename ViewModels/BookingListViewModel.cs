using beautix_bisp_17005.Models.Entities;

namespace beautix_bisp_17005.ViewModels
{
    public class BookingListViewModel
    {
        public int BookingId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string SalonName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public BookingStatus Status { get; set; }
        public bool CanCancel { get; set; }
        public int CreditsRequired { get; set; }
    }
}