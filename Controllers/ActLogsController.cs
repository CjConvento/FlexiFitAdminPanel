using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using FlexiFit_AdminPanel.Models;

namespace FlexiFit_AdminPanel.Controllers
{
    public class ActLogsController : Controller
    {
        private readonly string _connectionString;

        public ActLogsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("FlexifitDb")
                ?? throw new InvalidOperationException("Connection string 'FlexifitDb' not found.");
        }

        public async Task<IActionResult> Index(string search, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            Console.WriteLine("🔍 ActLogsController.Index() called");

            try
            {
                const int pageSize = 20;

                using var connection = new SqlConnection(_connectionString);
                Console.WriteLine("📡 Opening connection to Azure SQL...");

                var parameters = new DynamicParameters();

                string sql = @"
                    SELECT 
                        a.user_id,
                        u.username,
                        u.email,
                        a.activity_type,
                        a.activity_date,
                        a.details
                    FROM (
                        SELECT 
                            s.user_id,
                            'Workout' AS activity_type,
                            CAST(s.completed_at AS DATE) AS activity_date,
                            CONCAT('Completed workout: ', w.workout_name, ' (Day ', s.workout_day, ')') AS details
                        FROM usr_user_workout_sessions s
                        INNER JOIN usr_user_session_workouts sw ON s.session_id = sw.session_id
                        INNER JOIN wrk_workouts w ON sw.workout_id = w.workout_id
                        WHERE s.status = 'Completed'
                    
                        UNION ALL
                    
                        SELECT 
                            d.user_id,
                            'Nutrition' AS activity_type,
                            d.plan_date AS activity_date,
                            CONCAT('Logged meals: ', d.calories_consumed, ' kcal consumed, ', d.calories_burned, ' kcal burned') AS details
                        FROM ntr_daily_logs d
                        WHERE d.marked_done_at IS NOT NULL
                    
                        UNION ALL
                    
                        SELECT 
                            w.user_id,
                            'Water' AS activity_type,
                            w.log_date AS activity_date,
                            CONCAT('Logged ', w.water_ml, ' ml water') AS details
                        FROM ntr_water_logs w
                    ) a
                    INNER JOIN usr_users u ON a.user_id = u.user_id
                    WHERE 1=1
                ";

                // Apply filters
                if (!string.IsNullOrEmpty(search))
                {
                    sql += " AND (u.username LIKE @search OR u.email LIKE @search OR a.details LIKE @search)";
                    parameters.Add("@search", $"%{search}%");
                }
                if (fromDate.HasValue)
                {
                    sql += " AND a.activity_date >= @fromDate";
                    parameters.Add("@fromDate", fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    sql += " AND a.activity_date <= @toDate";
                    parameters.Add("@toDate", toDate.Value);
                }

                sql += " ORDER BY a.activity_date DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
                parameters.Add("@offset", (page - 1) * pageSize);
                parameters.Add("@pageSize", pageSize);

                var logs = await connection.QueryAsync<ActivityLogItem>(sql, parameters);

                string countSql = @"
                    SELECT COUNT(*)
                    FROM (
                        SELECT a.user_id
                        FROM (
                            SELECT s.user_id, CAST(s.completed_at AS DATE) AS activity_date
                            FROM usr_user_workout_sessions s
                            INNER JOIN usr_user_session_workouts sw ON s.session_id = sw.session_id
                            INNER JOIN wrk_workouts w ON sw.workout_id = w.workout_id
                            WHERE s.status = 'Completed'
                            UNION ALL
                            SELECT d.user_id, d.plan_date AS activity_date
                            FROM ntr_daily_logs d
                            WHERE d.marked_done_at IS NOT NULL
                            UNION ALL
                            SELECT w.user_id, w.log_date AS activity_date
                            FROM ntr_water_logs w
                        ) a
                        INNER JOIN usr_users u ON a.user_id = u.user_id
                        WHERE 1=1
                ";

                if (!string.IsNullOrEmpty(search))
                {
                    countSql += " AND (u.username LIKE @search OR u.email LIKE @search)";
                }
                if (fromDate.HasValue)
                {
                    countSql += " AND a.activity_date >= @fromDate";
                }
                if (toDate.HasValue)
                {
                    countSql += " AND a.activity_date <= @toDate";
                }

                countSql += " ) AS total";

                var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
                ViewBag.Search = search;
                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

                Console.WriteLine($"✅ Successfully fetched {logs.Count()} activity logs");
                return View(logs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching activity logs: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["Error"] = $"Failed to load activity logs: {ex.Message}";
                return View(new List<ActivityLogItem>());
            }
        }
    }
}