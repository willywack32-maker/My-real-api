using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Services.AddLogging();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"🔍 Connection string present: {!string.IsNullOrEmpty(connectionString)}");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ CONNECTION STRING IS NULL OR EMPTY!");
}
else
{
    Console.WriteLine($"🔍 Connection string starts with: {connectionString.Substring(0, Math.Min(20, connectionString.Length))}...");
}

builder.Services.AddDbContext<PickeAPIContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// ✅ ENHANCED AUTO-MIGRATION CODE WITH BETTER LOGGING
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🔄 Starting database migration process...");
        
        var dbContext = services.GetRequiredService<PickeAPIContext>();
        
        // Test connection first
        logger.LogInformation("🔌 Testing database connection...");
        var canConnect = await dbContext.Database.CanConnectAsync();
        logger.LogInformation($"📊 Database connection test: {canConnect}");
        
        if (!canConnect)
        {
            logger.LogError("❌ Cannot connect to database. Check connection string.");
            // Don't proceed with migrations if we can't connect
        }
        else
        {
            // Check migrations
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
            
            logger.LogInformation($"📦 Applied migrations: {appliedMigrations.Count()}");
            logger.LogInformation($"📦 Pending migrations: {pendingMigrations.Count()}");
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation($"🔄 Applying {pendingMigrations.Count()} migrations...");
                foreach (var migration in pendingMigrations)
                {
                    logger.LogInformation($"📋 Would apply: {migration}");
                }
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("✅ Database migrations completed successfully!");
            }
            else
            {
                logger.LogInformation("ℹ️ No pending migrations found.");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ MIGRATION FAILED: An error occurred during database migration");
    }
}

// Test endpoints
app.MapGet("/", () => "API Root - Working!");
app.MapGet("/test", () => "Test endpoint - Working!");
app.MapGet("/db-test", async (PickeAPIContext dbContext) => 
{
    try 
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        return $"Database test: {(canConnect ? "✅ CONNECTED" : "❌ FAILED")}";
    }
    catch (Exception ex)
    {
        return $"Database test: ❌ ERROR - {ex.Message}";
    }
});

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.Run();