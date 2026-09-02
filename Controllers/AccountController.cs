using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Net.Http; 
using System.Net.Http.Json; 
using FlexiFit_AdminPanel.Models;
using Google.Apis.Auth.OAuth2.Responses;

namespace FlexiFit_AdminPanel.Controllers
{
    public class AccountController : Controller
    {
        // ✅ BAGO: HttpClient factory para gumawa ng HTTP requests
        private readonly IHttpClientFactory _httpClientFactory;
        // ✅ BAGO: Base URL ng API (galing sa appsettings.json)
        private readonly string _apiBaseUrl;
        // ✅ BAGO: Logger para sa debugging at monitoring
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration, 
            ILogger<AccountController> logger)  // ✅ Add ILogger parameter
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("API Base URL not configured.");
            _logger = logger;  // ✅ Assign logger
        }

        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("📄 Login page loaded");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation($"🔍 Login attempt for username: {model.Username ?? "null"}");

            // Local Login (Username/Password) — pero iba na ang implementation
            if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.Password))
            {
                _logger.LogInformation($"🔐 Local login attempt for: {model.Username}");
                return await LoginWithPassword(model.Username, model.Password);
            }

            _logger.LogWarning("❌ No login credentials provided");
            ModelState.AddModelError(string.Empty, "Please provide valid credentials");
            return View(model);
        }

        private async Task<IActionResult> LoginWithPassword(string username, string password)
        {
            try
            {
                _logger.LogInformation("🔐 Attempting API login for username: {Username}", username);

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_apiBaseUrl);

                var response = await httpClient.PostAsJsonAsync("api/auth/admin-login", new
                {
                    Username = username,
                    Password = password
                });

                if (!response.IsSuccessStatusCode)
                {
                    // ✅ Huwag i-log ang buong error body — baka may sensitive data
                    _logger.LogWarning("❌ API login failed with status code: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Invalid credentials.");
                    return View("Login");
                }

                var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

                if (auth == null || string.IsNullOrEmpty(auth.Token))
                {
                    _logger.LogWarning("❌ Auth response missing token for user: {Username}", username);
                    ModelState.AddModelError(string.Empty, "Authentication failed.");
                    return View("Login");
                }

                if (auth.Role?.ToUpper() != "ADMIN")
                {
                    _logger.LogWarning("❌ Non-admin login attempt by: {Username} (Role: {Role})", username, auth.Role);
                    ModelState.AddModelError(string.Empty, "Access denied. Admin privileges required.");
                    return View("Login");
                }

                return await CreateAdminSession(auth);
            }
            catch (Exception ex)
            {
                // ✅ Safe: Huwag i-log ang ex.Message — gumamit ng structured logging
                _logger.LogError(ex, "❌ Login error for user: {Username}", username);
                ModelState.AddModelError(string.Empty, "An error occurred during login.");
                return View("Login");
            }
        }

        // ✅ BAGONG CreateAdminSession() — may JWT storage sa Session
        private async Task<IActionResult> CreateAdminSession(AuthResponse auth)
        {
            _logger.LogInformation($"✅ Creating admin session for: {auth.Name ?? auth.UserId.ToString()}");

            // ✅ BAGO: I-store ang JWT sa Session para magamit ng ibang controllers
            HttpContext.Session.SetString("JwtToken", auth.Token);

            // ✅ PINANATILI: Cookie session (gaya ng dati)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, auth.Name ?? auth.UserId.ToString()),
                new Claim(ClaimTypes.Role, auth.Role ?? "ADMIN"),
                new Claim("UserId", auth.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            _logger.LogInformation("✅ Session created, redirecting to Dashboard.");
            return RedirectToAction("Index", "Home");
        }
    
        // ✅ PINANATILI: Logout — may dagdag na pagbura ng JWT sa Session
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("👋 User logged out");
            
            // ✅ BAGO: Burahin din ang JWT sa Session
            HttpContext.Session.Remove("JwtToken");
            
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}