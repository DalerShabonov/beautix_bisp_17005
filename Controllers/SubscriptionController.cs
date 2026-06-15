using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.Services.Interfaces;
using beautix_bisp_17005.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace beautix_bisp_17005.Controllers
{
    /// <summary>
    /// Subscription pages: the public Plans/pricing page (anyone can view) plus the
    /// Activate and Dashboard actions which require a logged-in Subscriber. Note the
    /// [Authorize] sits on individual actions here, not the whole controller, so the
    /// pricing page stays visible to anonymous visitors.
    /// </summary>
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

        // GET: /Subscription/Plans — the pricing page.
        // If the visitor is logged in, we flag which plan is their current one so
        // the view can highlight it (e.g. disable the "choose" button on it).
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

        // POST: /Subscription/Activate — subscribe the current user to a plan.
        // (Payment is simulated; this just records the active subscription.)
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

        // GET: /Subscription/Dashboard — shows the subscriber their plan, credit
        // usage and renewal date. The view model copes with "no active plan" too.
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