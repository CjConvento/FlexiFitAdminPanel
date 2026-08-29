using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using FlexiFit_AdminPanel.Models;
using Microsoft.Extensions.Configuration;  // Add this

namespace FlexiFit_AdminPanel.Controllers
{
    public class WorkoutsController : Controller
    {
        private readonly string _connectionString;

        // Constructor injection – gets configuration from appsettings.json
        public WorkoutsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("FlexifitDb")!;
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string 'FlexifitDb' not found in appsettings.json.");
        }

        // 1. READ: Ipakita ang lahat ng workouts kasama ang img_filename
        public async Task<IActionResult> Index()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"SELECT workout_id, workout_name, muscle_group, equipment, environment, 
                            category, difficulty_level, is_weighted, notes, calories_burned, 
                            is_active, created_at, updated_at, img_filename, duration 
                            FROM dbo.wrk_workouts";

                var workouts = await connection.QueryAsync<WorkoutItem>(sql);
                return View(workouts);
            }
        }

        // 2. CREATE (GET): Ipakita ang form para sa bagong workout
        public IActionResult Create() => View();

        // 3. CREATE (POST): I-save ang bagong workout sa database
        [HttpPost]
        public async Task<IActionResult> Create(WorkoutItem workout)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Pansinin: Direktang '0' ang nilagay natin sa @is_active para laging Inactive ang bago
                var sql = @"INSERT INTO dbo.wrk_workouts 
                    (workout_name, muscle_group, equipment, environment, category, 
                     difficulty_level, is_weighted, notes, calories_burned, 
                     is_active, img_filename, duration, created_at, updated_at) 
                    VALUES 
                    (@workout_name, @muscle_group, @equipment, @environment, @category, 
                     @difficulty_level, @is_weighted, @notes, @calories_burned, 
                     0, @img_filename, @duration, GETDATE(), GETDATE())";

                await connection.ExecuteAsync(sql, workout);
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. EDIT (GET): Kunin ang data ng workout para i-edit
        public async Task<IActionResult> Edit(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "SELECT * FROM dbo.wrk_workouts WHERE workout_id = @Id";
                var workout = await connection.QueryFirstOrDefaultAsync<WorkoutItem>(sql, new { Id = id });
                return View(workout);
            }
        }

        // 7. WORKOUT TUTORIALS: Show workouts with video links
        public async Task<IActionResult> Tutorials()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"SELECT workout_id, workout_name, video_url 
                    FROM dbo.wrk_workouts 
                    WHERE video_url IS NOT NULL AND video_url != ''";
                var workouts = await connection.QueryAsync<WorkoutItem>(sql);
                return View(workouts);
            }
        }

        // 5. EDIT (POST): I-update ang existing workout record
        [HttpPost]
        public async Task<IActionResult> Edit(WorkoutItem workout)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"UPDATE dbo.wrk_workouts 
                            SET workout_name = @workout_name, 
                                muscle_group = @muscle_group, 
                                equipment = @equipment, 
                                environment = @environment, 
                                category = @category, 
                                difficulty_level = @difficulty_level, 
                                is_weighted = @is_weighted, 
                                notes = @notes, 
                                calories_burned = @calories_burned, 
                                is_active = @is_active, 
                                img_filename = @img_filename, 
                                duration = @duration,
                                updated_at = GETDATE()
                            WHERE workout_id = @workout_id";

                await connection.ExecuteAsync(sql, workout);
            }
            return RedirectToAction(nameof(Index));
        }

        // 6. DELETE: Burahin ang workout base sa ID
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "DELETE FROM dbo.wrk_workouts WHERE workout_id = @Id";
                await connection.ExecuteAsync(sql, new { Id = id });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}