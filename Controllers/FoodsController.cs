using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using FlexiFit_AdminPanel.Models; // <--- Siguraduhin na nandito ito

namespace FlexiFit_AdminPanel.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Dapper;
    using FlexiFit_AdminPanel.Models;

    public class FoodsController : Controller
    {
        private readonly string _connectionString = "Server=192.168.1.246,1433;Database=FLEXIFIT;User Id=cy;Password=********;TrustServerCertificate=True;";

        // READ: Ipakita ang lahat ng pagkain
        public async Task<IActionResult> Index()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "SELECT food_id, food_name, category, calories, protein_g as protein, carbs_g as carbs, fats_g as fats FROM dbo.ntr_food_items";
                var foods = await connection.QueryAsync<FoodItem>(sql);
                return View(foods);
            }
        }

        // CREATE: Ipakita ang form
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(FoodItem food)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"INSERT INTO dbo.ntr_food_items (food_name, category, calories, protein_g, carbs_g, fats_g) 
                        VALUES (@food_name, @category, @calories, @protein, @carbs, @fats)";
                await connection.ExecuteAsync(sql, food);
            }
            return RedirectToAction(nameof(Index));
        }

        // EDIT: Kunin ang data ng isang food para i-edit
        public async Task<IActionResult> Edit(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "SELECT food_id, food_name, category, calories, protein_g as protein, carbs_g as carbs, fats_g as fats FROM dbo.ntr_food_items WHERE food_id = @Id";
                var food = await connection.QueryFirstOrDefaultAsync<FoodItem>(sql, new { Id = id });
                return View(food);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FoodItem food)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"UPDATE dbo.ntr_food_items 
                        SET food_name = @food_name, category = @category, calories = @calories, 
                            protein_g = @protein, carbs_g = @carbs, fats_g = @fats 
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