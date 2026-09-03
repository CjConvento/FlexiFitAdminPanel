using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FlexiFit_AdminPanel.Models;

namespace FlexiFit_AdminPanel.Controllers
{
    [Authorize(Roles = "ADMIN")]  // ✅ Siguraduhin na ADMIN lang ang may access
    public class UsersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            ILogger<UsersController> logger
            )
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration["ApiUrl:BaseUrl"]
                ?? throw new InvalidOperationException("API Base URL not configured.");
            _logger = logger;
        }

        // ✅ HELPER: Gumawa ng HttpClient na may JWT token
        private async Task<HttpClient?> CreateAuthorizedClientAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("❌ No JWT token found in session.");
                return null;
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiBaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        // ✅ HELPER: I-handle ang 401 Unauthorized
        private IActionResult HandleUnauthorized()
        {
            _logger.LogWarning("⛔ Unauthorized access detected. Redirecting to Login.");
            return RedirectToAction("Login", "Account");
        }

        // 1. DISPLAY ALL USERS
        public async Task<IActionResult> Index(string searchTerm)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Fetching users from API...");

                var response = await client.GetAsync("api/users");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    // ✅ Huwag i-log ang buong error body
                    _logger.LogError("❌ API error: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Unable to fetch users.");
                    return View(new List<User>());
                }

                var users = await response.Content.ReadFromJsonAsync<List<User>>();
                _logger.LogInformation("✅ Retrieved {Count} users.", users?.Count ?? 0);

                // Optional: Search
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    users = users?.Where(u =>
                        u.username?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                        u.email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true
                    ).ToList();
                }

                return View(users ?? new List<User>());
            }
            catch (Exception ex)
            {
                // ✅ Safe: structured logging
                _logger.LogError(ex, "❌ Error fetching users");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching users.");
                return View(new List<User>());
            }
        }

        // 2. GET: CREATE PAGE
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. POST: PROCESS CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                if (string.IsNullOrEmpty(user.firebase_uid))
                {
                    user.firebase_uid = "ADMIN-GEN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                }

                user.auth_provider = user.auth_provider?.ToUpper() ?? "EMAIL";
                user.role = user.role?.ToUpper() ?? "USER";

                // ✅ Safe: email lang
                _logger.LogInformation("📡 Creating user with email: {Email}", user.email);

                var response = await client.PostAsJsonAsync("api/users/admin-create", user);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API create failed: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Failed to create user.");
                    return View(user);
                }

                TempData["Success"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating user with email: {Email}", user.email);
                ModelState.AddModelError(string.Empty, "API is unreachable. Please check the connection.");
                return View(user);
            }
        }

        // 4. GET: EDIT PAGE
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation($"📡 Fetching user ID {id} for edit");

                var response = await client.GetAsync($"api/users/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API returned {StatusCode} when fetching user {Id}", response.StatusCode, id);                    TempData["Error"] = $"User not found (ID: {id})";
                    return RedirectToAction(nameof(Index));
                }

                var user = await response.Content.ReadFromJsonAsync<User>();
                if (user == null) return NotFound();

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching user ID: {UserId}", id);
                TempData["Error"] = "Unable to connect to API. Please check the connection.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. POST: UPDATE USER
        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Updating user ID: {UserId}", user.user_id);

                var response = await client.PutAsJsonAsync($"api/users/{user.user_id}", user);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API update failed: {StatusCode} for user ID: {UserId}", response.StatusCode, user.user_id);
                    ModelState.AddModelError(string.Empty, "Update failed.");
                    return View(user);
                }

                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating user ID: {UserId}", user.user_id);
                ModelState.AddModelError(string.Empty, "API is unreachable. Please check the connection.");
                return View(user);
            }
        }

        // 6. DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Deleting user ID: {UserId}", id);

                var response = await client.DeleteAsync($"api/users/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API delete failed: {StatusCode} for user ID: {UserId}", response.StatusCode, id);
                    TempData["Error"] = "Unable to delete user. It might be in use.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "User deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting user ID: {UserId}", id);
                TempData["Error"] = "Connection to API failed during delete.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}