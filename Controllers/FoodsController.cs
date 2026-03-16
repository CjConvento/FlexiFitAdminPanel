using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using FlexiFit_AdminPanel.Models;

namespace FlexiFit_AdminPanel.Controllers
{
    public class FoodsController : Controller
    {
        private readonly string _connectionString = "Server=192.168.1.246,1433;Database=FLEXIFIT;User Id=cy;Password=********;TrustServerCertificate=True;";

        // 1. READ: Isama ang img_filename para gumana ang FullImageUrl logic
        public async Task<IActionResult> Index()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // DAGDAG: Isama ang is_active sa SELECT statement
                var sql = @"SELECT food_id, food_name, category, calories, 
                    protein_g as protein, carbs_g as carbs, fats_g as fats, 
                    img_filename, is_active FROM dbo.ntr_food_items";

                var foods = await connection.QueryAsync<FoodItem>(sql);
                return View(foods);
            }
        }

            // CREATE: Ipakita ang form
            public IActionResult Create() => View();

            // 3a. CREATE (POST): Isama ang img_filename sa pag-insert ng bagong record
            [HttpPost]
            public async Task<IActionResult> Create(FoodItem food)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                // Pansinin: '0' ang nilagay natin para sa is_active para sa developer review
                // Idinagdag din natin ang GETDATE() para sa audit logs
                var sql = @"INSERT INTO dbo.ntr_food_items 
                    (food_name, category, calories, protein_g, carbs_g, fats_g, img_filename, is_active, created_at, updated_at) 
                    VALUES 
                    (@food_name, @category, @calories, @protein, @carbs, @fats, @img_filename, 0, GETDATE(), GETDATE())";

                await connection.ExecuteAsync(sql, food);
            }
                return RedirectToAction(nameof(Index));
            }

        // 2. EDIT (GET): Kunin ang img_filename para sa preview sa edit page
        public async Task<IActionResult> Edit(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "SELECT food_id, food_name, category, calories, protein_g as protein, carbs_g as carbs, fats_g as fats, img_filename FROM dbo.ntr_food_items WHERE food_id = @Id";
                var food = await connection.QueryFirstOrDefaultAsync<FoodItem>(sql, new { Id = id });
                return View(food);
            }
        }

        // 3b. EDIT (POST): Siguraduhing ma-update ang img_filename sa database
        [HttpPost]
        public async Task<IActionResult> Edit(FoodItem food)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
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
            }
            return RedirectToAction(nameof(Index));
        }

        // DELETE: Burahin ang record
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "DELETE FROM dbo.ntr_food_items WHERE food_id = @Id";
                await connection.ExecuteAsync(sql, new { Id = id });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}