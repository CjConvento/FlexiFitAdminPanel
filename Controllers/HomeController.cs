using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FlexiFit_AdminPanel.Models;
using Microsoft.Extensions.Logging;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiBaseUrl;
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HomeController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<HomeController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _apiBaseUrl = configuration["ApiUrl:BaseUrl"]
            ?? throw new InvalidOperationException("API Base URL not configured.");
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    // ============================================================
    // 📊 DASHBOARD - Gumagamit na ng API
    // ============================================================
    public async Task<IActionResult> Index()
    {
        try
        {
            // 1. Kunin ang JWT token mula sa session
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No JWT token found in session.");
                ViewBag.TotalUsers = 0;
                ViewBag.TotalWorkouts = 0;
                ViewBag.TotalFoods = 0;
                return View();
            }

            // 2. Gumawa ng HttpClient at i-set ang Authorization header
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiBaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3. Call the API endpoint for dashboard stats
            var response = await client.GetAsync("api/admin/dashboard-stats");

            if (response.IsSuccessStatusCode)
            {
                var stats = await response.Content.ReadFromJsonAsync<DashboardStatsDto>();
                
                ViewBag.TotalUsers = stats?.TotalUsers ?? 0;
                ViewBag.TotalWorkouts = stats?.TotalWorkouts ?? 0;
                ViewBag.TotalFoods = stats?.TotalFoods ?? 0;
                
                _logger.LogInformation("✅ Dashboard stats loaded: Users={Users}, Workouts={Workouts}, Foods={Foods}", 
                    stats?.TotalUsers, stats?.TotalWorkouts, stats?.TotalFoods);
            }
            else
            {
                _logger.LogWarning("⚠️ Dashboard API call failed: {StatusCode}", response.StatusCode);
                
                // Fallback: dummy data kung hindi available ang API
                ViewBag.TotalUsers = 0;
                ViewBag.TotalWorkouts = 0;
                ViewBag.TotalFoods = 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading dashboard");
            
            // Fallback: dummy data kung may error
            ViewBag.TotalUsers = 0;
            ViewBag.TotalWorkouts = 0;
            ViewBag.TotalFoods = 0;
        }

        return View();
    }

    // ============================================================
    // 🔒 SECURE DEVELOPER TOOLS ROUTE
    // ============================================================
    // Layer 1 Security: Server-side check para sa naka-login na Admin user lamang.
    // Kapag may hindi autorisadong sumubok pumasok, automatic silang ihaharang ng .NET at ibabalik sa login page.
    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public IActionResult DeveloperTools()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        string? accessToken = session?.GetString("JwtToken");

        ViewBag.DeveloperJwtToken = accessToken ?? "Walang nahanap na token sa server session.";
        return View();
    }
}