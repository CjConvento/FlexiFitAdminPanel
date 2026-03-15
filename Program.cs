using Microsoft.Data.SqlClient; // Ito ang nawawalang linya
using Dapper;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Sa loob ng Program.cs, bago ang builder.Build()
var connectionString = builder.Configuration.GetConnectionString("FlexifitDb");

// Kung gagamit ka ng SQL Client
builder.Services.AddScoped<SqlConnection>(provider => new SqlConnection(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
