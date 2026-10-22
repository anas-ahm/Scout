using ScoutLibrary.Repositories;
using Activity = ScoutLibrary.Models.Activity;
using ScoutLibrary.Models;
using ScoutLibrary.Service;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<ScoutLibrary.DBWorker>();
builder.Services.AddScoped<ICRUD<Activity>, ActivityRepository>();
builder.Services.AddScoped<ICRUD<Member>, MemberRepository>();
builder.Services.AddScoped<ICRUD<Badge>, BadgeRepository>();
builder.Services.AddScoped<ScoutService>();
builder.Services.AddScoped<IMemberAuth, MemberRepository>();
builder.Services.AddScoped<IMemberAuthService, MemberAuthService>();
builder.Services.AddTransient<ActivityService>();
builder.Services.AddTransient<BadgeService>();


// Add cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
    {
        options.Cookie.Name = "session-cookie"; // Set the name of the cookie
        options.Cookie.HttpOnly = true; // Prevent client side scripts from accessing the cookie
        // options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Require https
        options.Cookie.SameSite = SameSiteMode.Lax; // Restricts the cookie to same-site requests
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Set the expiration time for the cookie
        options.SlidingExpiration = true; // Increases the expiration time if the user is still active
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();