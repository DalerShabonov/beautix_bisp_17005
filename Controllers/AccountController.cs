using beautix_bisp_17005.Models.Entities;
using beautix_bisp_17005.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace beautix_bisp_17005.Controllers
{
    /// <summary>
    /// Handles registration, login and logout. It's a thin layer over ASP.NET
    /// Identity's managers — we don't store passwords ourselves; Identity hashes
    /// and verifies them. After auth, users are routed to the right home page for
    /// their role.
    /// </summary>
    public class AccountController : Controller
    {
        // Identity services injected via DI: UserManager (create/find users),
        // SignInManager (sign in/out, password checks), RoleManager (roles).
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: /Account/Register — show the sign-up form (skip if already logged in).
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Register — handle the submitted sign-up form.
        // [ValidateAntiForgeryToken] protects against CSRF: the form must carry a
        // matching anti-forgery token or the request is rejected.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Server-side validation (required fields, email format, etc.).
            if (!ModelState.IsValid)
                return View(model);

            // Whitelist the role the user can self-assign so nobody can register
            // as "Admin" by tampering with the form. Anything unexpected -> Subscriber.
            var allowedRoles = new[] { "Subscriber", "SalonPartner" };
            if (!allowedRoles.Contains(model.Role))
                model.Role = "Subscriber";

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign the role, then sign them in immediately.
                await _userManager.AddToRoleAsync(user, model.Role);
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Send each role to its natural first step: partners set up a
                // salon, subscribers pick a plan.
                if (model.Role == "SalonPartner")
                    return RedirectToAction("Create", "Salon");

                return RedirectToAction("Plans", "Subscription");
            }

            // Registration failed (e.g. weak password / duplicate email) — surface
            // Identity's error messages on the form.
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Identity verifies the password. lockoutOnFailure: true means repeated
            // wrong attempts count towards the 5-strike lockout configured in Program.cs.
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Route to the dashboard that matches the user's role.
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Index", "Admin");

                    if (await _userManager.IsInRoleAsync(user, "SalonPartner"))
                        return RedirectToAction("Dashboard", "Salon");
                }

                // Honour a safe local returnUrl (e.g. they were redirected to log
                // in). Url.IsLocalUrl blocks open-redirect attacks to other sites.
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Dashboard", "Subscription");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Your account has been locked after too many failed attempts. Please try again in 15 minutes.");
                return View(model);
            }

            // Generic message on purpose — we don't reveal whether the email exists.
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // POST: /Account/Logout — POST + token so a malicious page can't log the
        // user out via a stray link. [Authorize] = must be signed in to call it.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}