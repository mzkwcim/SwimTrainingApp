using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using SwimTrainingApp.Data;
using SwimTrainingApp.Models;
using SwimTrainingApp.Validation;
using System.Globalization;

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine));

builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Dodanie uwierzytelniania opartego na ciasteczkach
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.Cookie.HttpOnly = true; // Zapobiega dostêpowi do ciasteczek z JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Wymaga HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict; // Chroni przed atakami CSRF
    });

builder.Services.AddAuthorization();

builder.Services.AddValidatorsFromAssemblyContaining<TrainingValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SwimTrainingAPI",
        Version = "v1"
    });
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CoachOnly", policy => policy.RequireRole("Coach"));
    options.AddPolicy("AthleteOnly", policy => policy.RequireRole("Athlete"));
});
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SwimTrainingAPI v1");
        c.RoutePrefix = "swagger";
    });
}

// Middleware dla uwierzytelniania i autoryzacji
app.UseAuthentication();
app.UseAuthorization();

// Punkty koñcowe API
app.MapGet("/api/users", async (AppDbContext dbContext) =>
{
    return await dbContext.Users.ToListAsync();
}).WithTags("Users");

app.MapGet("/api/users/{id}", async (AppDbContext dbContext, int id) =>
{
    var user = await dbContext.Users.FindAsync(id);
    return user == null ? Results.NotFound() : Results.Ok(user);
});

app.MapPost("/api/users", async (AppDbContext dbContext, User user) =>
{
    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", user);
});

app.MapPut("/api/users/{id}", async (AppDbContext dbContext, int id, User updatedUser) =>
{
    var user = await dbContext.Users.FindAsync(id);
    if (user == null)
    {
        return Results.NotFound();
    }

    user.Username = updatedUser.Username;
    user.Password = updatedUser.Password;
    user.Role = updatedUser.Role;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/users/{id}", async (AppDbContext dbContext, int id) =>
{
    var user = await dbContext.Users.FindAsync(id);
    if (user == null)
    {
        return Results.NotFound();
    }

    dbContext.Users.Remove(user);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/api/secure-data", () =>
{
    return "This is secured data";
}).RequireAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
