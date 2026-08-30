using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using FlexiFit_AdminPanel.Models;
using Microsoft.Extensions.Logging; 

namespace FlexiFit_AdminPanel.Controllers
{
    public class FoodsController : Controller
    {
        private readonly string _connectionString = "FlexiFitDb";
        private readonly ILogger<FoodsController> _logger; 

        // ✅ Constructor — basahin mula sa configuration
        public FoodsController(IConfiguration configuration, ILogger<FoodsController> logger)
        {
            _connectionString = configuration.GetConnectionString("FlexifitDb")
                ?? throw new InvalidOperationException("Connection string 'FlexifitDb' not found.");
            _logger = logger;
        }

        // 1. READ
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("📡 Fetching foods from database...");
                using var connection = new SqlConnection(_connectionString);
                
                var sql = @"SELECT food_id, food_name, category, calories, 
                            protein_g as protein, carbs_g as carbs, fats_g as fats, 
                            img_filename, is_active FROM dbo.ntr_food_items";

                var foods = await connection.QueryAsync<FoodItem>(sql);
                _logger.LogInformation("✅ Successfully fetched {Count} foods", foods.Count());
                return View(foods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching foods");
                TempData["Error"] = $"Failed to load foods: {ex.Message}";
                return View(new List<FoodItem>());
            }
        }

        // 2. CREATE (GET)
        public IActionResult Create() => View();

        // 3. CREATE (POST)
        [HttpPost]
        public async Task<IActionResult> Create(FoodItem food)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = @"INSERT INTO dbo.ntr_food_items 
                            (food_name, category, calories, protein_g, carbs_g, fats_g, img_filename, is_active, created_at, updated_at) 
                            VALUES 
                            (@food_name, @category, @calories, @protein, @carbs, @fats, @img_filename, 0, GETDATE(), GETDATE())";

                await connection.ExecuteAsync(sql, food);
                _logger.LogInformation("✅ Food created: {FoodName}", food.food_name);
                TempData["Success"] = "Food added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating food");
                ModelState.AddModelError(string.Empty, $"Failed to create food: {ex.Message}");
                return View(food);
            }
        }

        // 4. EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = "SELECT food_id, food_name, category, calories, protein_g as protein, carbs_g as carbs, fats_g as fats, img_filename FROM dbo.ntr_food_items WHERE food_id = @Id";
                var food = await connection.QueryFirstOrDefaultAsync<FoodItem>(sql, new { Id = id });
                
                if (food == null)
                {
                    TempData["Error"] = "Food not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(food);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching food for edit: {Id}", id);
                TempData["Error"] = $"Failed to load food: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. EDIT (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(FoodItem food)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = @"UPDATE dbo.ntr_food_items 
                            SET food_name = @food_name, 
                                category = @category, 
                                calories = @calories, 
                                protein_g = @protein, 
                                carbs_g = @carbs, 
                                fats_g = @fats,
                                img_filename = @img_filename 
                            WHERE food_id = @food_id";
                await connection.ExecuteAsync(sql, food);
                _logger.LogInformation("✅ Food updated: {FoodName}", food.food_name);
                TempData["Success"] = "Food updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating food: {Id}", food.food_id);
                ModelState.AddModelError(string.Empty, $"Failed to update food: {ex.Message}");
                return View(food);
            }
        }

        // 6. DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = "DELETE FROM dbo.ntr_food_items WHERE food_id = @Id";
                await connection.ExecuteAsync(sql, new { Id = id });
                _logger.LogInformation("✅ Food deleted: {Id}", id);
                TempData["Success"] = "Food deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting food: {Id}", id);
                TempData["Error"] = $"Failed to delete food: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}