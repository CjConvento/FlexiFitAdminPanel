using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Authorization;

public class HomeController : Controller
{
    private readonly string _connectionString;

    public HomeController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("FlexifitDb")!;
    }

    public async Task<IActionResult> Index()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // 1. Bilangin ang Users
            var totalUsers = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.usr_users");

            // 2. Bilangin ang Workouts
            var totalWorkouts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.wrk_workouts");

            // 3. Bilangin ang Foods
            var totalFoods = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.ntr_food_items");

            // I-pasa lahat sa ViewBag para mabasa ng HTML
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalWorkouts = totalWorkouts;
            ViewBag.TotalFoods = totalFoods;

            return View();
        }
    }

    // =======================================================
    // 🔒 BAGONG ADDITION: SECURE DEVELOPER TOOLS ROUTE
    // =======================================================
    // Layer 1 Security: Server-side check para sa naka-login na Admin user lamang.
    // Kapag may hindi autorisadong sumubok pumasok, automatic silang ihaharang ng .NET at ibabalik sa login page.
    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public IActionResult DeveloperTools()
    {
        string? accessToken = HttpContext.Session.GetString("JwtToken");
        ViewBag.DeveloperJwtToken = accessToken ?? "Walang nahanap na token sa server session.";
        return View();
    }
}