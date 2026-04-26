using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services.Interfaces;
using beautix_bisp_17005.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace beautix_bisp_17005.Controllers
{
    [Authorize(Roles = "Subscriber")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(
            IBookingService bookingService,
            ISubscriptionService subscriptionService,
            UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        // GET: /Booking/Browse
        [HttpGet]
        public async Task<IActionResult> Browse()
        {
            var userId = _userManager.GetUserId(User)!;
            var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);

            if (subscription == null)
            {
                TempData["Error"] = "You need an active subscription to book services.";
                return RedirectToAction("Plans", "Subscription");
            }

            var salons = await _bookingService.GetApprovedSalonsWithServicesAsync();
            return View(salons);
        }

        // GET: /Booking/Book/5
        [HttpGet]
        public async Task<IActionResult> Book(int serviceId)
        {
            var userId = _userManager.GetUserId(User)!;
            var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);

            if (subscription == null)
            {
                TempData["Error"] = "You need an active subscription to book services.";
                return RedirectToAction("Plans", "Subscription");
            }

            var viewModel = await _bookingService.GetBookingFormAsync(serviceId);

            if (viewModel == null)
            {
                TempData["Error"] = "The selected service is not available.";
                return RedirectToAction("Browse");
            }

            var hasCredits = await _subscriptionService
                .HasSufficientCreditsAsync(userId, viewModel.CreditsRequired);

            if (!hasCredits)
            {
                TempData["Error"] = "You do not have enough service credits remaining this month.";
                return RedirectToAction("Browse");
            }

            return View(viewModel);
        }

        // POST: /Booking/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User)!;

            var (success, message) = await _bookingService
                .CreateBookingAsync(userId, model.ServiceId, model.AppointmentDate);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction("MyBookings");
        }

        // GET: /Booking/MyBookings
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User)!;
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return View(bookings);
        }

        // POST: /Booking/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var userId = _userManager.GetUserId(User)!;

            var (success, message) = await _bookingService
                .CancelBookingAsync(userId, bookingId);

            if (success)
                TempData["Success"] = message;
            else
                TempData["Error"] = message;

            return RedirectToAction("MyBookings");
        }
    }
}