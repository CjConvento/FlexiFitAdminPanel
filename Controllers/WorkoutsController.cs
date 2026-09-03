using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FlexiFit_AdminPanel.Models;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;  // Add this

namespace FlexiFit_AdminPanel.Controllers
{
    public class WorkoutsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;
        private readonly ILogger<WorkoutsController> _logger; 

        // Constructor injection – gets configuration from appsettings.json
        public WorkoutsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<WorkoutsController> logger) 
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"]
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
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        // ✅ HELPER: I-handle ang 401 Unauthorized
        private IActionResult HandleUnauthorized()
        {
            _logger.LogWarning("⛔ Unauthorized access detected. Redirecting to Login.");
            return RedirectToAction("Login", "Account");
        }

        // 1. INDEX: Ipakita ang lahat ng workouts
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Fetching all workouts from API...");

                var response = await client.GetAsync("api/workout/admin/all");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogError("❌ API error: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Unable to fetch workouts.");
                    return View(new List<WorkoutItem>());
                }

                var workouts = await response.Content.ReadFromJsonAsync<List<WorkoutItem>>();
                _logger.LogInformation("✅ Retrieved {Count} workouts.", workouts?.Count ?? 0);

                return View(workouts ?? new List<WorkoutItem>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching workouts");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching workouts.");
                return View(new List<WorkoutItem>());
            }
        }

        // 2. CREATE (GET): Ipakita ang form para sa bagong workout
        [HttpGet]
        public IActionResult Create() => View();

        // 3. POST: PROCESS CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkoutItem workout)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Creating new workout: {Name}", workout.workout_name);

                var response = await client.PostAsJsonAsync("api/workout/admin/create", workout);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API create failed: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Failed to create workout.");
                    return View(workout);
                }

                TempData["Success"] = "Workout created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating workout: {Name}", workout.workout_name);
                ModelState.AddModelError(string.Empty, "API is unreachable. Please check the connection.");
                return View(workout);
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

                _logger.LogInformation("📡 Fetching workout ID {Id} for edit", id);

                var response = await client.GetAsync($"api/workout/admin/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API returned {StatusCode} when fetching workout {Id}", response.StatusCode, id);
                    TempData["Error"] = $"Workout not found (ID: {id})";
                    return RedirectToAction(nameof(Index));
                }

                var workout = await response.Content.ReadFromJsonAsync<WorkoutItem>();
                if (workout == null) return NotFound();

                return View(workout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching workout ID: {Id}", id);
                TempData["Error"] = "Unable to connect to API. Please check the connection.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. POST: UPDATE WORKOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WorkoutItem workout)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Updating workout ID: {Id}", workout.workout_id);

                var response = await client.PutAsJsonAsync($"api/workout/admin/{workout.workout_id}", workout);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API update failed: {StatusCode} for workout ID: {Id}", response.StatusCode, workout.workout_id);
                    ModelState.AddModelError(string.Empty, "Update failed.");
                    return View(workout);
                }

                TempData["Success"] = "Workout updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating workout ID: {Id}", workout.workout_id);
                ModelState.AddModelError(string.Empty, "API is unreachable. Please check the connection.");
                return View(workout);
            }
        }

        // 6. POST: DELETE WORKOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Deleting workout ID: {Id}", id);

                var response = await client.DeleteAsync($"api/workout/admin/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogWarning("❌ API delete failed: {StatusCode} for workout ID: {Id}", response.StatusCode, id);
                    TempData["Error"] = "Unable to delete workout. It might be in use.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Workout deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting workout ID: {Id}", id);
                TempData["Error"] = "Connection to API failed during delete.";
            }

            return RedirectToAction(nameof(Index));
        }

        // 7. TUTORIALS: Show workouts with video links
        public async Task<IActionResult> Tutorials()
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleUnauthorized();

                _logger.LogInformation("📡 Fetching tutorials from API...");

                var response = await client.GetAsync("api/workout/admin/tutorials");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleUnauthorized();

                    _logger.LogError("❌ API error: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Unable to fetch tutorials.");
                    return View(new List<WorkoutItem>());
                }

                var tutorials = await response.Content.ReadFromJsonAsync<List<WorkoutItem>>();
                _logger.LogInformation("✅ Retrieved {Count} tutorials.", tutorials?.Count ?? 0);

                return View(tutorials ?? new List<WorkoutItem>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching tutorials");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching tutorials.");
                return View(new List<WorkoutItem>());
            }
        }

    }
}