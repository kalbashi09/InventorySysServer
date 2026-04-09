using Microsoft.EntityFrameworkCore;
using Data; 
using Services; 

var builder = WebApplication.CreateBuilder(args);

// 1. Register the Database (The Pantry)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Register your Application Logic (The Brains)
// ALL Services must be registered HERE (Before builder.Build())
builder.Services.AddScoped<InventoryService>(); // This is the "Recipe Book" for inventory management
builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<UserStartupService>(); // MOVED THIS UP



// 3. Register Controllers (The Gatekeepers)
builder.Services.AddControllers();

var app = builder.Build(); // The "Concrete" is poured here. No more changes allowed.

// 4. Configure the Middleware Pipeline
app.MapControllers();

app.Run();