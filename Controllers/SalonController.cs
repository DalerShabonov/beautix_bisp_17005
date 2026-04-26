using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services.Interfaces;
using beautix_bisp_17005.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace beautix_bisp_17005.Controllers
{
    [Authorize(Roles = "SalonPartner")]
    public class SalonController : Controller
    {
        private readonly ISalonService _salonService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalonController(
            ISalonService salonService,
            UserManager<ApplicationUser> userManager)
        {
            _salonService = salonService;
            _userManager = userManager;
        }

        // GET: /Salon/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User)!;
            var existing = await _salonService.GetSalonByOwnerAsync(userId);

            if (existing != null)
                return RedirectToAction("Dashboard");

            return View(new SalonCreateViewModel());
        }

        // POST: /Salon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalonCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User)!;
            var success = await _salonService.CreateSalonAsync(userId, model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty,
                    "You already have a registered salon.");
                return View(model);
            }

            TempData["Success"] =
                "Your salon has been submitted for approval. " +
                "You will be notified once it is approved by our team.";

            return RedirectToAction("Dashboard");
        }

        // GET: /Salon/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User)!;
            var dashboard = await _salonService.GetDashboardAsync(userId);

            if (dashboard == null)
                return RedirectToAction("Create");

            return View(dashboard);
        }

        // GET: /Salon/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User)!;
            var salon = await _salonService.GetSalonByOwnerAsync(userId);

            if (salon == null)
                return RedirectToAction("Create");

            var model = new SalonCreateViewModel
            {
                Name = salon.Name,
                Address = salon.Address,
                District = salon.District,
                ContactEmail = salon.ContactEmail,
                ContactPhone = salon.ContactPhone,
                Description = salon.Description
            };

            ViewData["SalonId"] = salon.Id;
            return View(model);
        }

        // POST: /Salon/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int salonId, SalonCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["SalonId"] = salonId;
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;
            var success = await _salonService.UpdateSalonAsync(salonId, userId, model);

            if (!success)
            {
                TempData["Error"] = "Unable to update salon details. Please try again.";
                return View(model);
            }

            TempData["Success"] = "Salon details updated successfully.";
            return RedirectToAction("Dashboard");
        }

        // POST: /Salon/AddService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(
            int salonId, ServiceCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Please check the service details and try again.";
                return RedirectToAction("Dashboard");
            }

            var userId = _userManager.GetUserId(User)!;
            var success = await _salonService.AddServiceAsync(salonId, userId, model);

            if (!success)
                TempData["Error"] = "Unable to add service. Please try again.";
            else
                TempData["Success"] = "Service added successfully.";

            return RedirectToAction("Dashboard");
        }

        // POST: /Salon/ToggleService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleService(int serviceId)
        {
            var userId = _userManager.GetUserId(User)!;
            await _salonService.ToggleServiceAvailabilityAsync(serviceId, userId);
            TempData["Success"] = "Service availability updated.";
            return RedirectToAction("Dashboard");
        }

        // POST: /Salon/DeleteService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int serviceId)
        {
            var userId = _userManager.GetUserId(User)!;
            var success = await _salonService.DeleteServiceAsync(serviceId, userId);

            if (!success)
                TempData["Error"] = "Unable to delete service.";
            else
                TempData["Success"] = "Service deleted successfully.";

            return RedirectToAction("Dashboard");
        }
    }
}