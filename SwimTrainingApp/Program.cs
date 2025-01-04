using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SwimTrainingApp.Data;
using SwimTrainingApp.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SwimTrainingAPI v1");
    c.RoutePrefix = string.Empty; 
});

app.MapGet("/seed", async (AppDbContext db) =>
{
    if (!db.Users.Any())
    {
        db.Users.Add(new User { Username = "admin", Password = "admin123", Role = UserRole.Admin });
        db.Users.Add(new User { Username = "coach", Password = "coach123", Role = UserRole.Coach });
        db.Users.Add(new User { Username = "athlete", Password = "athlete123", Role = UserRole.Athlete });
        await db.SaveChangesAsync();
    }
    return "Database seeded with initial data.";
});
app.MapGet("/", () => "Hello World!");

app.MapGet("/api/users", async (AppDbContext db) =>
{
    var users = await db.Users.ToListAsync();
    return users.Select(u => new { u.Id, u.Username, u.Role });
});

app.MapGet("/api/trainings", async (AppDbContext db) =>
{
    var trainings = await db.Trainings.Include(t => t.Tasks).ToListAsync();
    return trainings;
});

app.MapPost("/api/trainings", async (Training training, AppDbContext db) =>
{
    db.Trainings.Add(training);
    await db.SaveChangesAsync();
    return Results.Created($"/api/trainings/{training.Id}", training);
});

app.MapGet("/testdb", async (AppDbContext db) =>
{
    var count = await db.Users.CountAsync();
    return $"Database contains {count} users.";
});

app.Run();
