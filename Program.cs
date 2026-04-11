using Microsoft.EntityFrameworkCore;
using Data; 
using Services; 

var builder = WebApplication.CreateBuilder(args);

// 1. Database - Ensure your connection string in appsettings.json is correct for PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Dependency Injection
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<UserStartupService>();
builder.Services.AddScoped<LicenseService>();
builder.Services.AddSingleton<AuthService>();

// 3. Background Services
builder.Services.AddHostedService<ExpirationWorker>();

// 4. API Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// 5. CORS - Mandatory for your Tunnel and Mobile connections
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => 
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 6. Network Binding - Essential for your Tunnel to find your laptop
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps());
    options.ListenAnyIP(7227, listenOptions => listenOptions.UseHttps());
});

var app = builder.Build();

// 7. Middleware Pipeline
// Move Swagger outside the 'if' if you want it to work via the Tunnel on "Production" mode
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API V1");
    c.RoutePrefix = "swagger"; // This ensures it stays at /swagger
});

app.UseCors("AllowAll");
app.UseHttpsRedirection(); // Standard practice
app.UseAuthorization();
app.MapControllers();
   
app.Run();