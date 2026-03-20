using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using FlexiFit_AdminPanel.Models;

namespace FlexiFit_AdminPanel.Controllers
{
    public class AccountController : Controller
    {
        // Hindi muna natin kailangan ang HttpClient para sa login na ito
        public AccountController() { }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // 1. Dito mo i-set ang gusto mong Admin credentials
            // Dahil Firebase ang gamit sa mobile, itong Admin panel ay may sariling "Master Key"
            string hardcodedAdmin = "admin";
            string hardcodedPass = "flexifit";

            // 2. I-check kung tumutugma ang input
            if (model.Username == hardcodedAdmin && model.Password == hardcodedPass)
            {
                // 3. Gawa ng Claims para malaman ng system na "Admin" ka
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "SuperAdmin"),
                    new Claim(ClaimTypes.Role, "ADMIN")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // 4. I-save ang session sa browser (Cookies)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // 5. Redirect sa Dashboard
                return RedirectToAction("Index", "Home");
            }

            // Pag mali ang credentials
            ModelState.AddModelError(string.Empty, "Invalid Admin Username or Password.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}