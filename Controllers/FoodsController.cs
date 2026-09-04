using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FlexiFit_AdminPanel.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

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
            _apiBaseUrl = configuration["ApiUrl:BaseUrl"]
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

        // ✅ HELPER: Handle expired token with message
        private IActionResult HandleExpiredToken()
        {
            _logger.LogWarning("⛔ Token expired. Redirecting to Login.");
            TempData["Error"] = "Your session has expired. Please login again.";
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Login", "Account");
        }

        // 🔧 Helper method para mag-upload sa Blob
        private async Task<string> UploadImageToBlob(IFormFile file, string container)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            using var formData = new MultipartFormDataContent();
            using var streamContent = new StreamContent(file.OpenReadStream());
            formData.Add(streamContent, "file", file.FileName);

            var response = await client.PostAsync($"{_apiBaseUrl}/api/blob/upload?container={container}", formData);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BlobUploadResult>(json);
                return result?.fileName ?? throw new Exception("Upload succeeded but no fileName returned.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Your session has expired. Please login again.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Upload failed: {error}");
            }
        }

        // 1. INDEX: Ipakita ang lahat ng foods
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleExpiredToken();

                _logger.LogInformation("📡 Fetching all foods from API...");

                var response = await client.GetAsync("api/nutrition/admin/foods");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleExpiredToken();

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
        public async Task<IActionResult> Create(FoodItem food, IFormFile imageFile)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleExpiredToken();

                // ✅ KUNG MAY IMAGE NA IN-UPLOAD
                if (imageFile != null && imageFile.Length > 0)
                {
                    var container = "foods";
                    var fileName = await UploadImageToBlob(imageFile, container);
                    food.img_filename = fileName;
                }

                _logger.LogInformation("📡 Creating new food: {Name}", food.food_name);

                var response = await client.PostAsJsonAsync("api/nutrition/admin/foods", food);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleExpiredToken();

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
                if (client == null) return HandleExpiredToken();

                _logger.LogInformation("📡 Fetching food ID {Id} for edit", id);

                var response = await client.GetAsync($"api/nutrition/admin/foods/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleExpiredToken();

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
        public async Task<IActionResult> Edit(FoodItem food, IFormFile imageFile)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleExpiredToken();

                // ✅ KUNG MAY BAGONG IMAGE NA IN-UPLOAD
                if (imageFile != null && imageFile.Length > 0)
                {
                    var container = "foods";
                    var fileName = await UploadImageToBlob(imageFile, container);
                    food.img_filename = fileName;
                }

                _logger.LogInformation("📡 Updating food ID: {Id}", food.food_id);

                var response = await client.PutAsJsonAsync($"api/nutrition/admin/foods/{food.food_id}", food);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleExpiredToken();

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
                if (client == null) return HandleExpiredToken();

                _logger.LogInformation("📡 Deleting food ID: {Id}", id);

                var response = await client.DeleteAsync($"api/nutrition/admin/foods/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleExpiredToken();

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