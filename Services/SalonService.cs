using beautix_bisp_17005.Data;
using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services.Interfaces;
using beautix_bisp_17005.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace beautix_bisp_17005.Services
{
    /// <summary>
    /// All salon-side operations for a SalonPartner: creating/editing their salon,
    /// managing services, and building the dashboard. Every method that changes
    /// data also checks ownership (OwnerUserId == ownerId) so partners can only
    /// ever touch their own salon and services.
    /// </summary>
    public class SalonService : ISalonService
    {
        private readonly ApplicationDbContext _context;

        public SalonService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Finds the salon belonging to a given owner (a partner has at most one).
        public async Task<Salon?> GetSalonByOwnerAsync(string ownerId)
        {
            return await _context.Salons
                .Include(s => s.Services)
                .FirstOrDefaultAsync(s => s.OwnerUserId == ownerId);
        }

        // Registers a new salon. Returns false if the partner already owns one,
        // enforcing the "one salon per partner" rule. New salons start unapproved.
        public async Task<bool> CreateSalonAsync(string ownerId, SalonCreateViewModel model)
        {
            var existing = await GetSalonByOwnerAsync(ownerId);
            if (existing != null)
                return false;

            var salon = new Salon
            {
                Name = model.Name,
                Address = model.Address,
                District = model.District,
                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone,
                Description = model.Description,
                IsApproved = false,
                OwnerUserId = ownerId
            };

            await _context.Salons.AddAsync(salon);
            await _context.SaveChangesAsync();
            return true;
        }

        // Updates a salon's profile. The query filters on both salonId AND ownerId,
        // so a partner can't edit a salon that isn't theirs (note: this does not
        // change IsApproved — approval stays an admin-only decision).
        public async Task<bool> UpdateSalonAsync(
            int salonId, string ownerId, SalonCreateViewModel model)
        {
            var salon = await _context.Salons
                .FirstOrDefaultAsync(s => s.Id == salonId && s.OwnerUserId == ownerId);

            if (salon == null)
                return false;

            salon.Name = model.Name;
            salon.Address = model.Address;
            salon.District = model.District;
            salon.ContactEmail = model.ContactEmail;
            salon.ContactPhone = model.ContactPhone;
            salon.Description = model.Description;

            _context.Salons.Update(salon);
            await _context.SaveChangesAsync();
            return true;
        }

        // Builds the partner dashboard: the salon's profile, its services, summary
        // counts (total/confirmed/today's bookings) and the 10 most recent bookings.
        public async Task<SalonDashboardViewModel?> GetDashboardAsync(string ownerId)
        {
            var salon = await _context.Salons
                .Include(s => s.Services)
                .FirstOrDefaultAsync(s => s.OwnerUserId == ownerId);

            if (salon == null)
                return null;

            // Pull every booking made against any of this salon's services.
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Service)
                .Where(b => b.Service != null && b.Service.SalonId == salon.Id)
                .OrderByDescending(b => b.AppointmentDate)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;

            return new SalonDashboardViewModel
            {
                SalonId = salon.Id,
                SalonName = salon.Name,
                Address = salon.Address,
                District = salon.District,
                ContactEmail = salon.ContactEmail,
                ContactPhone = salon.ContactPhone,
                Description = salon.Description,
                IsApproved = salon.IsApproved,
                TotalBookings = bookings.Count,
                ConfirmedBookings = bookings.Count(b =>
                    b.Status == BookingStatus.Confirmed),
                TodaysBookings = bookings.Count(b =>
                    b.AppointmentDate.Date == today &&
                    b.Status == BookingStatus.Confirmed),
                RecentBookings = bookings.Take(10).Select(b =>
                    new SalonBookingItemViewModel
                    {
                        BookingId = b.Id,
                        CustomerName = b.User.FullName,
                        ServiceName = b.Service!.Name,
                        AppointmentDate = b.AppointmentDate,
                        Status = b.Status.ToString()
                    }).ToList(),
                Services = salon.Services.Select(sv =>
                    new SalonServiceViewModel
                    {
                        ServiceId = sv.Id,
                        Name = sv.Name,
                        Description = sv.Description,
                        DurationMinutes = sv.DurationMinutes,
                        CreditsRequired = sv.CreditsRequired,
                        Price = sv.Price,
                        IsAvailable = sv.IsAvailable
                    }).ToList()
            };
        }

        // Adds a service to the partner's salon (after confirming they own it).
        public async Task<bool> AddServiceAsync(
            int salonId, string ownerId, ServiceCreateViewModel model)
        {
            var salon = await _context.Salons
                .FirstOrDefaultAsync(s => s.Id == salonId && s.OwnerUserId == ownerId);

            if (salon == null)
                return false;

            var service = new Service
            {
                Name = model.Name,
                Description = model.Description,
                DurationMinutes = model.DurationMinutes,
                CreditsRequired = model.CreditsRequired,
                Price = model.Price,
                IsAvailable = true,
                SalonId = salonId
            };

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
            return true;
        }

        // Flips a service between available/hidden. Joins through Salon so we can
        // verify the requesting partner actually owns the service's salon.
        public async Task<bool> ToggleServiceAvailabilityAsync(
            int serviceId, string ownerId)
        {
            var service = await _context.Services
                .Include(s => s.Salon)
                .FirstOrDefaultAsync(s => s.Id == serviceId &&
                                         s.Salon.OwnerUserId == ownerId);

            if (service == null)
                return false;

            service.IsAvailable = !service.IsAvailable;
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
            return true;
        }

        // Permanently removes a service (again, only if the partner owns it).
        // Existing bookings survive — their ServiceId is set to null by the DB rule.
        public async Task<bool> DeleteServiceAsync(int serviceId, string ownerId)
        {
            var service = await _context.Services
                .Include(s => s.Salon)
                .FirstOrDefaultAsync(s => s.Id == serviceId &&
                                         s.Salon.OwnerUserId == ownerId);

            if (service == null)
                return false;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}