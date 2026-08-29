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
using Microsoft.Extensions.Logging;  // ✅ Add this!

namespace FlexiFit_AdminPanel.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<AccountController> _logger;  // ✅ Add this!

        public AccountController(IConfiguration configuration, ILogger<AccountController> logger)  // ✅ Add ILogger parameter
        {
            _connectionString = configuration.GetConnectionString("FlexifitDb")
                ?? throw new InvalidOperationException("Connection string not found.");
            _logger = logger;  // ✅ Assign logger
        }

        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("📄 Login page loaded");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation($"🔍 Login attempt for username: {model.Username ?? "null"}");

            // 🔥 Firebase Token Login (PRIMARY)
            if(!string.IsNullOrEmpty(model.FirebaseToken))
            {
                _logger.LogInformation("🔥 Firebase token detected, attempting Firebase login");
                return await LoginWithFirebase(model.FirebaseToken);
            }

            // 🔐 Local Login (FALLBACK)
            if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.Password))
            {
                _logger.LogInformation($"🔐 Local login attempt for: {model.Username}");
                return await LoginWithPassword(model.Username, model.Password);
            }

            _logger.LogWarning("❌ No login credentials provided");
            ModelState.AddModelError(string.Empty, "Please provide valid credentials");
            return View(model);
        }

        private async Task<IActionResult> LoginWithFirebase(string firebaseToken)
        {
            try
            {
                _logger.LogInformation("🔥 Verifying Firebase token...");
                var decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(firebaseToken);
                
                var firebaseUid = decodedToken.Uid;
                var email = decodedToken.Claims.GetValueOrDefault("email")?.ToString();

                _logger.LogInformation($"🔥 Firebase user: UID={firebaseUid}, Email={email}");

                using (var connection = new SqlConnection(_connectionString))
                {
                    var sql = @"
                        SELECT user_id, username, email, role, firebase_uid, name
                        FROM dbo.usr_users
                        WHERE firebase_uid = @FirebaseUid OR email = @Email";

                    var user = await connection.QueryFirstOrDefaultAsync<User>(
                        sql, new { FirebaseUid = firebaseUid, Email = email });

                    if (user == null)
                    {
                        _logger.LogWarning($"❌ Firebase user not found in database: {firebaseUid}");
                        ModelState.AddModelError(string.Empty, "User not registered in the system.");
                        return View("Login");
                    }

                    _logger.LogInformation($"✅ Firebase user found: {user.username} (Role: {user.role})");

                    if (user.role?.ToUpper() != "ADMIN")
                    {
                        _logger.LogWarning($"❌ Firebase user {user.username} is not ADMIN");
                        ModelState.AddModelError(string.Empty, "Access denied. Admin privileges required.");
                        return View("Login");
                    }

                    return await CreateAdminSession(user);
                }
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError($"❌ Firebase authentication failed: {ex.Message}");
                ModelState.AddModelError(string.Empty, $"Authentication failed: {ex.Message}");
                return View("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Firebase login error: {ex.Message}");
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View("Login");
            }
        }

        private async Task<IActionResult> LoginWithPassword(string username, string password)
        {
            _logger.LogInformation($"🔍 Login attempt for username: {username}");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    _logger.LogInformation("📡 Opening database connection...");

                    var sql = @"
                        SELECT user_id, username, email, role, firebase_uid, name, password_hash 
                        FROM dbo.usr_users 
                        WHERE username = @Username AND status = 'active'";

                    _logger.LogInformation($"📝 Executing SQL query for username: {username}");

                    var user = await connection.QueryFirstOrDefaultAsync<User>(
                        sql, new { Username = username });

                    if (user == null)
                    {
                        _logger.LogWarning($"❌ User not found: {username}");
                        ModelState.AddModelError(string.Empty, "Invalid credentials.");
                        return View("Login");
                    }

                    _logger.LogInformation($"✅ User found: {user.username}, Role: {user.role}, ID: {user.user_id}");

                    // Check kung ADMIN
                    if (user.role?.ToUpper() != "ADMIN")
                    {
                        _logger.LogWarning($"❌ User {username} is not an ADMIN (Role: {user.role})");
                        ModelState.AddModelError(string.Empty, "Admin access required.");
                        return View("Login");
                    }

                    // Verify password hash
                    if (!string.IsNullOrEmpty(user.password_hash))
                    {
                        _logger.LogInformation($"🔐 Verifying password hash for user: {username}");
                        if (!PasswordHelper.VerifyPassword(password, user.password_hash))
                        {
                            _logger.LogWarning($"❌ Password verification failed for user: {username}");
                            ModelState.AddModelError(string.Empty, "Invalid credentials.");
                            return View("Login");
                        }
                        _logger.LogInformation($"✅ Password verified for user: {username}");
                    }
                    else
                    {
                        _logger.LogInformation($"🔐 Creating password hash for user: {username}");
                        var hashedPassword = PasswordHelper.HashPassword(password);
                        await UpdatePasswordHash(user.user_id, hashedPassword);
                    }

                    _logger.LogInformation($"✅ Creating session for user: {username}");
                    return await CreateAdminSession(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Login error for {username}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, "An error occurred during login.");
                return View("Login");
            }
        }

        private async Task<IActionResult> CreateAdminSession(User user)
        {
            _logger.LogInformation($"✅ Creating admin session for: {user.username} (ID: {user.user_id})");

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

            _logger.LogInformation($"✅ Session created for: {user.username}, redirecting to Dashboard");
            return RedirectToAction("Index", "Home");
        }

        private async Task UpdatePasswordHash(int userId, string hashedPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "UPDATE dbo.usr_users SET password_hash = @Hash WHERE user_id = @UserId";
                await connection.ExecuteAsync(sql, new { Hash = hashedPassword, UserId = userId });
                _logger.LogInformation($"✅ Password hash updated for user ID: {userId}");
            }
        }
    
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("👋 User logged out");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}