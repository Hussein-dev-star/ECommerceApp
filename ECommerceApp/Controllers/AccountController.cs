using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;

namespace ECommerceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]


[HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ModelState.Remove("Role");

        if (ModelState.IsValid)
        {
            var savedCart = HttpContext.Session.GetString("ShoppingCart");

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);

                if (!string.IsNullOrEmpty(savedCart))
                {
                    HttpContext.Session.SetString("ShoppingCart", savedCart);
                }

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Product");
                }

                ModelState.AddModelError(string.Empty, "بريد إلكتروني أو كلمة مرور خاطئة!");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            HttpContext.Session.Remove("ShoppingCart");

            return RedirectToAction("Index", "Home");
        }
    }
}