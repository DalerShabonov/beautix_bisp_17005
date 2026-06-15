using beautix_bisp_17005.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace beautix_bisp_17005.Controllers
{
    /// <summary>
    /// The admin back-office. The whole controller is locked to the "Admin" role,
    /// so none of these pages or actions are reachable by ordinary users. Read
    /// actions (Index/Users/Salons) are GET; state changes (approve/suspend/lock)
    /// are POST + anti-forgery token.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: /Admin/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dashboard = await _adminService.GetDashboardAsync();
            return View(dashboard);
        }

        // GET: /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        // GET: /Admin/Salons
        [HttpGet]
        public async Task<IActionResult> Salons()
        {
            var salons = await _adminService.GetAllSalonsAsync();
            return View(salons);
        }

        // POST: /Admin/ApproveSalon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSalon(int salonId)
        {
            var success = await _adminService.ApproveSalonAsync(salonId);

            if (success)
                TempData["Success"] = "Salon approved successfully. It is now live on the platform.";
            else
                TempData["Error"] = "Unable to approve salon. Please try again.";

            return RedirectToAction("Salons");
        }

        // POST: /Admin/SuspendSalon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendSalon(int salonId)
        {
            var success = await _adminService.SuspendSalonAsync(salonId);

            if (success)
                TempData["Success"] = "Salon suspended and removed from the platform.";
            else
                TempData["Error"] = "Unable to suspend salon. Please try again.";

            return RedirectToAction("Salons");
        }

        // POST: /Admin/LockUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string userId)
        {
            await _adminService.LockUserAsync(userId);
            TempData["Success"] = "User account locked.";
            return RedirectToAction("Users");
        }

        // POST: /Admin/UnlockUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            await _adminService.UnlockUserAsync(userId);
            TempData["Success"] = "User account unlocked.";
            return RedirectToAction("Users");
        }
    }
}