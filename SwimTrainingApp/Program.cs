using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SwimTrainingApp.Data;
using SwimTrainingApp.Validation;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using SwimTrainingApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

var key = builder.Configuration["Jwt:Key"]; 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
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



var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

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



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SwimTrainingAPI v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
