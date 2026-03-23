using Microsoft.AspNetCore.Mvc;
using FlexiFit_AdminPanel.Models;
using System.Net.Http.Json;

namespace FlexiFit_AdminPanel.Controllers
{
    public class UsersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IHttpClientFactory httpClientFactory, ILogger<UsersController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            // Siguraduhing 5160 ang port ng iyong API project window
            _httpClient.BaseAddress = new Uri("http://localhost:5160/"); 
            _logger = logger;
        }

        // 1. DISPLAY ALL USERS
        public async Task<IActionResult> Index(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Admin Panel: Fetching users from API...");

                // Dagdagan natin ng log para makita kung ano ang saktong URL na tinatawag
                var response = await _httpClient.GetAsync("api/users");

                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<User>>();
                    _logger.LogInformation("Admin Panel: Successfully fetched {Count} users.", users?.Count ?? 0);

                    if (!string.IsNullOrEmpty(searchTerm) && users != null)
                    {
                        searchTerm = searchTerm.ToLower().Trim();
                        users = users.Where(u =>
                            (u.username?.ToLower().Contains(searchTerm) ?? false) ||
                            (u.email?.ToLower().Contains(searchTerm) ?? false) ||
                            (u.name?.ToLower().Contains(searchTerm) ?? false)
                        ).ToList();
                    }

                    return View(users ?? new List<User>());
                }
                else
                {
                    _logger.LogWarning("API Error: {Status}", response.StatusCode);
                    TempData["Error"] = $"API Error: {response.StatusCode}";
                    return View(new List<User>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Critical Connection Error: {Message}", ex.Message);
                TempData["Error"] = "Hindi makakonekta sa API. Pakisiguradong running ang API project sa port 5160.";
                return View(new List<User>());
            }
        }

        // 2. GET: CREATE PAGE
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 5. GET: EDIT PAGE
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("Fetching user ID {Id} for edit", id);
                var response = await _httpClient.GetAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<User>();
                    if (user == null)
                        return NotFound();

                    return View(user);
                }
                else
                {
                    _logger.LogWarning("API returned {StatusCode} when fetching user {Id}", response.StatusCode, id);
                    TempData["Error"] = $"User not found (ID: {id})";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {Id}", id);
                TempData["Error"] = "Unable to connect to API. Please ensure the API is running on port 5160.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 6. POST: UPDATE USER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            try
            {
                _logger.LogInformation("Updating user ID {Id}", user.user_id);
                var response = await _httpClient.PutAsJsonAsync($"api/users/{user.user_id}", user);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API update failed for user {Id}: {Error}", user.user_id, errorMsg);
                ModelState.AddModelError(string.Empty, $"Update failed: {errorMsg}");
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", user.user_id);
                ModelState.AddModelError(string.Empty, "API is unreachable. Please check the connection.");
                return View(user);
            }
        }

        // 3. POST: PROCESS CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            try
            {
                // A. Siguraduhing may Firebase UID (Important for API)
                if (string.IsNullOrEmpty(user.firebase_uid))
                {
                    user.firebase_uid = "ADMIN-GEN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                }

                // B. REVISE: Kunin ang provider mula sa form (EMAIL or GOOGLE)
                // Kung null, default natin sa EMAIL para safe sa SQL Constraint
                user.auth_provider = user.auth_provider?.ToUpper() ?? "EMAIL";

                // C. Default Role
                user.role = user.role?.ToUpper() ?? "USER";

                _logger.LogInformation("Sending request to API for: {Email} with Provider: {Provider}", user.email, user.auth_provider);

                var response = await _httpClient.PostAsJsonAsync("api/users/admin-create", user);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "User created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API Error Response: {Error}", errorMsg);
                ModelState.AddModelError(string.Empty, $"Failed: {errorMsg}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Critical Error connecting to API: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "API is unreachable.");
            }

            return View(user);
        }

        // 4. POST: DELETE USER (Dagdag ito para gumana ang Trash Icon)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting user ID: {Id}", id);
                var response = await _httpClient.DeleteAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "User deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Hindi mabura ang user. Baka ginagamit pa sa ibang records.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Delete Error: {Message}", ex.Message);
                TempData["Error"] = "Connection to API failed during delete.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}