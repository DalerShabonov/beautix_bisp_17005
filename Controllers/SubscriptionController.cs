using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services.Interfaces;
using beautix_bisp_17005.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace beautix_bisp_17005.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionController(
            ISubscriptionService subscriptionService,
            UserManager<ApplicationUser> userManager)
        {
            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        // GET: /Subscription/Plans
        [HttpGet]
        public async Task<IActionResult> Plans()
        {
            var plans = await _subscriptionService.GetAllPlansAsync();
            string? currentPlanId = null;

            if (User.Identity!.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User)!;
                var active = await _subscriptionService.GetActiveSubscriptionAsync(userId);
                currentPlanId = active?.PlanId.ToString();
            }

            var viewModels = plans.Select(p => new SubscriptionPlanViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                MonthlyPrice = p.MonthlyPrice,
                ServiceAllowance = p.ServiceAllowance,
                IsCurrentPlan = currentPlanId == p.Id.ToString()
            }).ToList();

            return View(viewModels);
        }

        // POST: /Subscription/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Subscriber")]
        public async Task<IActionResult> Activate(int planId)
        {
            var userId = _userManager.GetUserId(User)!;
            var success = await _subscriptionService.ActivateSubscriptionAsync(userId, planId);

            if (!success)
            {
                TempData["Error"] = "Unable to activate the selected plan. Please try again.";
                return RedirectToAction("Plans");
            }

            TempData["Success"] = "Your subscription has been activated successfully.";
            return RedirectToAction("Dashboard");
        }

        // GET: /Subscription/Dashboard
        [HttpGet]
        [Authorize(Roles = "Subscriber")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User)!;
            var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);

            var viewModel = new SubscriptionDashboardViewModel();

            if (subscription != null)
            {
                viewModel.HasActiveSubscription = true;
                viewModel.PlanName = subscription.Plan.Name;
                viewModel.MonthlyPrice = subscription.Plan.MonthlyPrice;
                viewModel.ServiceAllowance = subscription.Plan.ServiceAllowance;
                viewModel.ServicesUsed = subscription.ServicesUsed;
                viewModel.StartDate = subscription.StartDate;
                viewModel.RenewalDate = subscription.RenewalDate;
                viewModel.PaymentStatus = subscription.PaymentStatus;
            }

            return View(viewModel);
        }
    }
}