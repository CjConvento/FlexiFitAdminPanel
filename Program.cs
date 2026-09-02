using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FlexiFit_AdminPanel.Helpers; 

var builder = WebApplication.CreateBuilder(args);

// ========== ENVIRONMENT CHECK ==========
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

// ========== LOAD CONFIG ==========
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// ========== FIREBASE ADMIN SDK ==========
try
{
    // ✅ 1. UNAHIN ANG ENVIRONMENT VARIABLE (Para sa Azure)
    string? firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT");

    if (!string.IsNullOrEmpty(firebaseJson))
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromJson(firebaseJson)
        });
        Console.WriteLine("✅ Firebase initialized using environment variable.");
    }
    else
    {
        // ✅ 2. FALLBACK: BASAHIN MULA SA FILE (Para sa Local Development)
        var firebaseCredentialPath = builder.Configuration["Firebase:CredentialPath"] 
            ?? "Credentials/firebase-service-account.json";

        if (File.Exists(firebaseCredentialPath))
        {
            using var stream = File.OpenRead(firebaseCredentialPath);
            var credential = GoogleCredential.FromStream(stream);
            
            FirebaseApp.Create(new AppOptions()
            {
                Credential = credential
            });
            Console.WriteLine($"✅ Firebase initialized using file: {firebaseCredentialPath}");
        }
        else
        {
            // ⚠️ 3. KUNG WALA, MAG-LOG NG WARNING AT HUWAG I-INITIALIZE
            Console.WriteLine("⚠️ Firebase credentials not found. Skipping Firebase initialization.");
            Console.WriteLine("   Set FIREBASE_SERVICE_ACCOUNT environment variable or add Credentials/firebase-service-account.json file.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Firebase initialization failed: {ex.Message}");
    // Optional: throw kung kailangan talaga ang Firebase
    // throw;
}

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddLogging();

builder.Services.AddHttpContextAccessor();

// ========== HTTP CLIENT FOR BLAZOR ==========
builder.Services.AddScoped(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var request = httpContextAccessor.HttpContext?.Request;
    var baseUri = request != null 
        ? $"{request.Scheme}://{request.Host}" 
        : "http://localhost:5100/";
    
    return new HttpClient
    {
        BaseAddress = new Uri(baseUri)
    };
});

// ========== COOKIE AUTH ==========
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// ========== LOGGING CONFIGURATION ==========
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Ito ang magpapadala ng logs sa Log Stream
builder.Logging.AddDebug();   // Para sa local debugging

ApiUrlHelper.Configure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ========== REQUEST LOGGING (FOR DEBUGGING) ==========
app.Use(async (context, next) =>
{
    Console.WriteLine($"📨 {context.Request.Method} {context.Request.Path}");
    await next();
});

app.UseStaticFiles();
app.UseRouting();

// 2. SIGURADUHING NASA GITNA ITO NG ROUTING AT ENDPOINTS
app.UseAuthentication();
app.UseAuthorization();

// // ==========  BLAZOR ENDPOINTS ==========
// app.MapRazorPages();
// app.MapBlazorHub();

// ✅ MVC Routes — ensure na ang root path ay mapunta sa Account/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ✅ Fallback para sa mga request na hindi na-match
app.MapFallbackToController("Login", "Account");    

app.Run();