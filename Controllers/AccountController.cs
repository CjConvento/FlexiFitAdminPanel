using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using FlexiFit_AdminPanel.Models;
using FlexiFit_AdminPanel.Helpers;
using FirebaseAdmin.Auth;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Authorization;

namespace FlexiFit_AdminPanel.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;
        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("FlexifitDb")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // 🔥 Firebase Token Login (PRIMARY)
            if(!string.IsNullOrEmpty(model.FirebaseToken))
            {
                return await LoginWithFirebase(model.FirebaseToken);
            }

            // 🔐 Local Login (FALLBACK)
            if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.Password))
            {
                return await LoginWithPassword(model.Username, model.Password);
            }

            ModelState.AddModelError(string.Empty, "Please provide valid credentials");
            return View(model);
        }
        private async Task<IActionResult> LoginWithFirebase(string firebaseToken)
        {
            try
            {   var decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(firebaseToken);
                
                var firebaseUid = decodedToken.Uid;
                var email = decodedToken.Claims.GetValueOrDefault("email")?.ToString();

                using (var connection = new SqlConnection(_connectionString))
                {
                    var sql = @"
                        SELECT user_id, username, email, role, firebase_uid, name
                        FROM dbo.usr_users
                        WHERE firebase_uid = @FirebaseUid OR email = @Email";

                    var user = await connection.QueryFirstOrDefaultAsync<User>(
                        sql, new { FirebaseUid = firebaseUid, Email = email }
                    );

                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "User not registered in the system.");
                        return View("Login");
                    }

                    // ✅ Check kung ADMIN
                    if (user.role?.ToUpper() != "ADMIN")
                    {
                        ModelState.AddModelError(string.Empty, "Access denied. Admin privileges required.");
                        return View("Login");
                    }

                    return await CreateAdminSession(user);
                }
   
            }
            catch (FirebaseAuthException ex)
            {
                ModelState.AddModelError(string.Empty, $"Authentication failed: {ex.Message}");
                return View("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View("Login");
            }
        }
        private async Task<IActionResult> LoginWithPassword(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT user_id, username, email, role, firebase_uid, name, password_hash 
                    FROM dbo.usr_users 
                    WHERE username = @Username AND status = 'active'";

                var user = await connection.QueryFirstOrDefaultAsync<User>(
                    sql, new { Username = username }
                );

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials.");
                    return View("Login");
                }

                // Check kung ADMIN
                if (user.role?.ToUpper() != "ADMIN")
                {
                    ModelState.AddModelError(string.Empty, "Admin access required.");
                    return View("Login");
                }

                // Verify password hash
                if (!string.IsNullOrEmpty(user.password_hash))
                {
                    if (!PasswordHelper.VerifyPassword(password, user.password_hash))
                    {
                        ModelState.AddModelError(string.Empty, "Invalid credentials.");
                        return View("Login");
                    }
                }
                else
                {
                    // 🔐 UNANG MANUAL LOGIN — CREATE HASH!
                    var hashedPassword = PasswordHelper.HashPassword(password);
                    await UpdatePasswordHash(user.user_id, hashedPassword);
                    
                    // Update local object para magamit sa session
                    user.password_hash = hashedPassword;
                }

                return await CreateAdminSession(user);
            }
        }
        private async Task<IActionResult> CreateAdminSession(User user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.username ?? user.email),
                new Claim(ClaimTypes.Email, user.email ?? ""),
                new Claim(ClaimTypes.Role, user.role ?? "ADMIN"),
                new Claim("UserId", user.user_id.ToString()),
                new Claim("FirebaseUid", user.firebase_uid ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }
        private async Task UpdatePasswordHash(int userId, string hashedPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "UPDATE dbo.usr_users SET password_hash = @Hash WHERE user_id = @UserId";
                await connection.ExecuteAsync(sql, new { Hash = hashedPassword, UserId = userId });
            }
        }
    
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}