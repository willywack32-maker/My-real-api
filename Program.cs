using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL Database - PROPER connection string handling
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PickeAPIContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// ✅ ADD AUTO-MIGRATION CODE HERE
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<PickeAPIContext>();
        
        // Check for pending migrations
        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"Applying {pendingMigrations.Count()} pending migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully!");
        }
        else
        {
            Console.WriteLine("No pending migrations found.");
        }
        
        // Optional: Ensure database is created
        dbContext.Database.EnsureCreated();
        
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while applying migrations: {ex.Message}");
        // Don't throw - let the app start even if migrations fail
    }
}

// Test endpoints to verify API is working
app.MapGet("/", () => "API Root - Working!");
app.MapGet("/test", () => "Test endpoint - Working!");

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.Run();