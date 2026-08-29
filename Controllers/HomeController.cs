using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;

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
}