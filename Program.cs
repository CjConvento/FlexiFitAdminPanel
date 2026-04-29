using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
// Idagdag ito sa itaas

var builder = WebApplication.CreateBuilder(args);


// ========== ADD THESE TWO LINES ==========
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
var conn = builder.Configuration.GetConnectionString("FlexifitDb");
Console.WriteLine($"Connection string used: {conn}");
// =========================================

using (var testConnection = new SqlConnection(conn))
{
    try
    {
        testConnection.Open();
        Console.WriteLine("Database connection successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Direct connection failed: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
    }
}

using (var testConnection = new SqlConnection(conn))
{
    try
    {
        testConnection.Open();
        Console.WriteLine("Database connection successful!");

        // Query the usr_users table
        using (var command = new SqlCommand("SELECT COUNT(*) FROM usr_users", testConnection))
        {
            var count = command.ExecuteScalar();
            Console.WriteLine($"Number of users in usr_users: {count}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Query failed: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
    }
}

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddLogging();

// 1. ADD COOKIE AUTHENTICATION HERE
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect dito kapag hindi naka-login
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// 2. SIGURADUHING NASA GITNA ITO NG ROUTING AT ENDPOINTS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Default page is Login

app.Run();