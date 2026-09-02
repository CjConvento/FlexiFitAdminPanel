using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FlexiFit_AdminPanel.Models;

namespace FlexiFit_AdminPanel.Controllers
{
    public class FoodsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;
        private readonly ILogger<FoodsController> _logger; 

        // ✅ Constructor — basahin mula sa configuration
        public FoodsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration, 
            ILogger<FoodsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("API Base URL not configured.");
            _logger = logger;
        }

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
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        private IActionResult HandleUnauthorized()
        {
            _logger.LogWarning("⛔ Unauthorized access detected. Redirecting to Login.");
            return RedirectToAction("Login", "Account");
        }

        // 1. INDEX: Ipakita ang lahat ng foods
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Fetching all foods from API...");

                var response = await client.GetAsync("api/nutrition/admin/foods");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogError("❌ API error: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Unable to fetch foods.");
                    return View(new List<FoodItem>());
                }

                var foods = await response.Content.ReadFromJsonAsync<List<FoodItem>>();
                _logger.LogInformation("✅ Retrieved {Count} foods.", foods?.Count ?? 0);

                return View(foods ?? new List<FoodItem>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching foods");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching foods.");
                return View(new List<FoodItem>());
            }
        }

        // 2. CREATE (GET)
        public IActionResult Create() => View();

        // 3. POST: PROCESS CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodItem food)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Creating new food: {Name}", food.food_name);

                var response = await client.PostAsJsonAsync("api/nutrition/admin/foods", food);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API create failed: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Failed to create food.");
                    return View(food);
                }

                TempData["Success"] = "Food created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating food: {Name}", food.food_name);
                ModelState.AddModelError(string.Empty, "API is unreachable. Please check the connection.");
                return View(food);
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

                _logger.LogInformation("📡 Fetching food ID {Id} for edit", id);

                var response = await client.GetAsync($"api/nutrition/admin/foods/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API returned {StatusCode} when fetching food {Id}", response.StatusCode, id);
                    TempData["Error"] = $"Food not found (ID: {id})";
                    return RedirectToAction(nameof(Index));
                }

                var food = await response.Content.ReadFromJsonAsync<FoodItem>();
                if (food == null) return NotFound();

                return View(food);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching food ID: {Id}", id);
                TempData["Error"] = "Unable to connect to API. Please check the connection.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. POST: UPDATE FOOD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FoodItem food)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Updating food ID: {Id}", food.food_id);

                var response = await client.PutAsJsonAsync($"api/nutrition/admin/foods/{food.food_id}", food);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API update failed: {StatusCode} for food ID: {Id}", response.StatusCode, food.food_id);
                    ModelState.AddModelError(string.Empty, "Update failed.");
                    return View(food);
                }

                TempData["Success"] = "Food updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating food ID: {Id}", food.food_id);
                ModelState.AddModelError(string.Empty, "API is unreachable. Please check the connection.");
                return View(food);
            }
        }

        // 6. POST: DELETE FOOD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Deleting food ID: {Id}", id);

                var response = await client.DeleteAsync($"api/nutrition/admin/foods/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API delete failed: {StatusCode} for food ID: {Id}", response.StatusCode, id);
                    TempData["Error"] = "Unable to delete food. It might be in use.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Food deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting food ID: {Id}", id);
                TempData["Error"] = "Connection to API failed during delete.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}