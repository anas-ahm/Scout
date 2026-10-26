using ScoutLibrary.Repositories;
using Activity = ScoutLibrary.Models.Activity;
using ScoutLibrary.Models;
using ScoutLibrary.Service;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AuthorizeFolder("/Leder", "LeaderPolicy");
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Signup");
});
builder.Services.AddScoped<ScoutLibrary.DBWorker>();
builder.Services.AddScoped<ICRUD<Activity>, ActivityRepository>();
builder.Services.AddScoped<ICRUD<Member>, MemberRepository>();
builder.Services.AddScoped<ICRUD<Badge>, BadgeRepository>();
builder.Services.AddScoped<ScoutService>();
builder.Services.AddScoped<IMemberAuth, MemberRepository>();
builder.Services.AddScoped<IMemberAuthService, MemberAuthService>();
builder.Services.AddScoped<IMemberActivity, MemberRepository>();
builder.Services.AddTransient<ActivityService>();
builder.Services.AddTransient<BadgeService>();


// Add cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
    {
        options.LoginPath = "/log-ind";       //hvor du sendes hen hvis du prøver at tilgå en side der kræver login
        options.AccessDeniedPath = "/Index";  //hvor du sendes hen, hvis du er logget ind men prøver at tilgå en side du ikke er autoriseret til 
        options.Cookie.Name = "session-cookie"; // Set the name of the cookie
        options.Cookie.HttpOnly = true; // Prevent client side scripts from accessing the cookie
        // options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Require https
        options.Cookie.SameSite = SameSiteMode.Lax; // Restricts the cookie to same-site requests
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Set the expiration time for the cookie
        options.SlidingExpiration = true; // Increases the expiration time if the user is still active
    });

builder.Services.AddAuthorization(options=>         //laver autorisatonspolitikker, der bruges til at styre hvem der kan tilgå hvilke mapper/sider
{ 
    options.AddPolicy("LeaderPolicy", policy => policy.RequireRole("Leader"));
    options.AddPolicy("LeaderMemberPolicy", policy => policy.RequireRole("Leader", "Member"));
});

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