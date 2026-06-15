using beautix_bisp_17005.Data;
using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services.Interfaces;
using beautix_bisp_17005.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace beautix_bisp_17005.Services
{
    /// <summary>
    /// The booking workflow: browsing approved salons, preparing the booking form,
    /// creating a booking (with all the business-rule checks), listing a user's
    /// bookings, and cancelling. It leans on ISubscriptionService for the credit
    /// side of things rather than duplicating that logic.
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        // Composed dependency: booking needs to check/spend/refund credits.
        private readonly ISubscriptionService _subscriptionService;

        public BookingService(
            ApplicationDbContext context,
            ISubscriptionService subscriptionService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
        }

        // Builds the "browse" page: only approved salons, and within each, only
        // their available services. We project straight into view models so the
        // view gets exactly the fields it needs (and EF can do it in one query).
        public async Task<List<SalonBrowseViewModel>> GetApprovedSalonsWithServicesAsync()
        {
            return await _context.Salons
                .Where(s => s.IsApproved)
                .Include(s => s.Services.Where(sv => sv.IsAvailable))
                .Select(s => new SalonBrowseViewModel
                {
                    SalonId = s.Id,
                    SalonName = s.Name,
                    Address = s.Address,
                    District = s.District,
                    Description = s.Description,
                    Services = s.Services
                        .Where(sv => sv.IsAvailable)
                        .Select(sv => new ServiceItemViewModel
                        {
                            ServiceId = sv.Id,
                            ServiceName = sv.Name,
                            Description = sv.Description,
                            DurationMinutes = sv.DurationMinutes,
                            CreditsRequired = sv.CreditsRequired,
                            Price = sv.Price
                        }).ToList()
                })
                .ToListAsync();
        }

        // Prepares the data shown on the booking form for one service. Returns null
        // if the service doesn't exist or isn't available, so the controller can
        // redirect with a friendly message. Defaults the date to tomorrow.
        public async Task<BookingCreateViewModel?> GetBookingFormAsync(int serviceId)
        {
            var service = await _context.Services
                .Include(s => s.Salon)
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsAvailable);

            if (service == null)
                return null;

            return new BookingCreateViewModel
            {
                ServiceId = service.Id,
                ServiceName = service.Name,
                SalonName = service.Salon.Name,
                CreditsRequired = service.CreditsRequired,
                DurationMinutes = service.DurationMinutes,
                AppointmentDate = DateTime.UtcNow.AddDays(1)
            };
        }

        /// <summary>
        /// Creates a booking. Returns a (Success, Message) tuple so the controller
        /// can show the exact reason on failure instead of a generic error. The
        /// method walks a series of guard checks before committing anything.
        /// </summary>
        public async Task<(bool Success, string Message)> CreateBookingAsync(
            string userId, int serviceId, DateTime appointmentDate)
        {
            // PostgreSQL "timestamptz" comparisons require a UTC kind. The form
            // posts a local-looking time, so we stamp it as UTC for consistency.
            if (appointmentDate.Kind != DateTimeKind.Utc)
                appointmentDate = DateTime.SpecifyKind(appointmentDate, DateTimeKind.Utc);

            // 1) Can't book in the past.
            if (appointmentDate < DateTime.UtcNow)
                return (false, "Appointment date must be in the future.");

            // 2) The service must still exist and be available.
            var service = await _context.Services
                .Include(s => s.Salon)
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsAvailable);

            if (service == null)
                return (false, "The selected service is no longer available.");

            // 3) The salon must be approved (not suspended).
            if (!service.Salon.IsApproved)
                return (false, "This salon is not currently active on the platform.");

            // 4) The user must have enough credits left this month.
            var hasSufficientCredits = await _subscriptionService
                .HasSufficientCreditsAsync(userId, service.CreditsRequired);

            if (!hasSufficientCredits)
                return (false, "You do not have enough service credits remaining this month. Please upgrade your plan.");

            // 5) Prevent a double-booking at the exact same date/time.
            var conflict = await _context.Bookings
                .AnyAsync(b =>
                    b.UserId == userId &&
                    b.Status == BookingStatus.Confirmed &&
                    b.AppointmentDate == appointmentDate);

            if (conflict)
                return (false, "You already have a booking at this date and time.");

            // All checks passed — spend the credits first, then save the booking.
            var deducted = await _subscriptionService
                .DeductCreditAsync(userId, service.CreditsRequired);

            if (!deducted)
                return (false, "Unable to deduct service credits. Please try again.");

            var booking = new Booking
            {
                UserId = userId,
                ServiceId = serviceId,
                AppointmentDate = appointmentDate,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            return (true, "Your booking has been confirmed successfully.");
        }

        // Lists all of a user's bookings, newest first. ThenInclude pulls the
        // salon name through the service. Note the null-safe handling for services
        // that were deleted after the booking was made (ServiceId set to null).
        public async Task<List<BookingListViewModel>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Service)
                    .ThenInclude(s => s!.Salon)
                .OrderByDescending(b => b.AppointmentDate)
                .Select(b => new BookingListViewModel
                {
                    BookingId = b.Id,
                    ServiceName = b.Service != null ? b.Service.Name : "Service no longer available",
                    SalonName = b.Service != null ? b.Service.Salon.Name : "N/A",
                    AppointmentDate = b.AppointmentDate,
                    Status = b.Status,
                    CreditsRequired = b.Service != null ? b.Service.CreditsRequired : 0,
                    // Only show the cancel button when it's still allowed:
                    // confirmed and more than 24 hours away (matches the rule below).
                    CanCancel = b.Status == BookingStatus.Confirmed &&
                                b.AppointmentDate > DateTime.UtcNow.AddHours(24)
                })
                .ToListAsync();
        }

        /// <summary>
        /// Cancels a booking and refunds the credit. The userId is part of the
        /// lookup so one user can never cancel another user's booking.
        /// </summary>
        public async Task<(bool Success, string Message)> CancelBookingAsync(
            string userId, int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
                return (false, "Booking not found.");

            // Can't cancel something already cancelled/completed.
            if (booking.Status != BookingStatus.Confirmed)
                return (false, "Only confirmed bookings can be cancelled.");

            // Enforce the 24-hour cancellation policy.
            if (booking.AppointmentDate <= DateTime.UtcNow.AddHours(24))
                return (false, "Bookings cannot be cancelled within 24 hours of the appointment.");

            booking.Status = BookingStatus.Cancelled;
            _context.Bookings.Update(booking);

            // Give the credit back. Fall back to 1 if the service was removed.
            var creditsRequired = booking.Service?.CreditsRequired ?? 1;
            await _subscriptionService.ReinstateCreditAsync(userId, creditsRequired);

            await _context.SaveChangesAsync();

            return (true, "Your booking has been cancelled and your service credit has been reinstated.");
        }
    }
}