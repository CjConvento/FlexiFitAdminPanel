using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FlexiFit_AdminPanel.Models;

namespace FlexiFit_AdminPanel.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class ActLogsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;
        private readonly ILogger<ActLogsController> _logger;

        public ActLogsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ActLogsController> logger)
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

        private IActionResult HandleExpiredToken()
        {
            _logger.LogWarning("⛔ Token expired. Redirecting to Login.");
            TempData["Error"] = "Your session has expired. Please login again.";
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Login", "Account");
        }

        // 1. INDEX: Ipakita ang lahat ng activity logs (with filters)
        public async Task<IActionResult> Index(
            string? search = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleExpiredToken();

                _logger.LogInformation("📡 Fetching all activity logs from API...");

                // Build query parameters
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize=20");

                var url = "api/actlogs/admin/all";
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleExpiredToken();

                    _logger.LogError("❌ API error: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Unable to fetch activity logs.");
                    return View(new List<ActivityLogItem>());
                }

                var result = await response.Content.ReadFromJsonAsync<ActivityLogResponse>();
                var logs = result?.Data ?? new List<ActivityLogItem>();

                _logger.LogInformation("✅ Retrieved {Count} activity logs (Total: {Total})", logs.Count, result?.Total ?? 0);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = result?.TotalPages ?? 1;
                ViewBag.Search = search;
                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

                return View(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching activity logs");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching activity logs.");
                return View(new List<ActivityLogItem>());
            }
        }

        // 2. GET: DETAILS PAGE
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleExpiredToken();

                _logger.LogInformation("📡 Fetching activity log ID {Id} for details", id);

                var response = await client.GetAsync($"api/actlogs/admin/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleExpiredToken();

                    _logger.LogWarning("❌ API returned {StatusCode} when fetching log {Id}", response.StatusCode, id);
                    TempData["Error"] = $"Activity log not found (ID: {id})";
                    return RedirectToAction(nameof(Index));
                }

                var log = await response.Content.ReadFromJsonAsync<ActivityLogItem>();
                if (log == null) return NotFound();

                return View(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching activity log ID: {Id}", id);
                TempData["Error"] = "Unable to connect to API. Please check the connection.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. POST: DELETE ACTIVITY LOG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = await CreateAuthorizedClientAsync();
                if (client == null) return HandleExpiredToken();

                _logger.LogInformation("📡 Deleting activity log ID: {Id}", id);

                var response = await client.DeleteAsync($"api/actlogs/admin/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return HandleExpiredToken();

                    _logger.LogWarning("❌ API delete failed: {StatusCode} for log ID: {Id}", response.StatusCode, id);
                    TempData["Error"] = "Unable to delete activity log.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Activity log deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting activity log ID: {Id}", id);
                TempData["Error"] = "Connection to API failed during delete.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}