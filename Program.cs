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

builder.Services.AddDbContext<PickeAPIContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// ✅ OPTION 2: DATABASE SETUP CODE - Replace your current migration code with this
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🔄 Setting up database...");
        
        var dbContext = services.GetRequiredService<PickeAPIContext>();
        
        // Ensure database is created and tables are built
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("✅ Database tables ensured!");
        
        // Test if we can query the Pickers table
        var pickerCount = await dbContext.Pickers.CountAsync();
        logger.LogInformation($"📊 Current Pickers in database: {pickerCount}");
        
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Database setup failed");
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

// Add this test endpoint too
app.MapGet("/test-picker", async (PickeAPIContext dbContext) => 
{
    try 
    {
        // Try to create and read a test picker
        var testPicker = new Picker 
        { 
            Name = "Test Picker", 
            AppleType = "Apple Type",
            OrchardName = "Test Orchard",
            HoursWorked = "hours worked",
            BinRate = "bin rate",
            PackHouse = "Test PackHouse"
        };
        
        dbContext.Pickers.Add(testPicker);
        await dbContext.SaveChangesAsync();
        
        var pickers = await dbContext.Pickers.ToListAsync();
        return $"✅ Database working! Pickers count: {pickers.Count}";
    }
    catch (Exception ex)
    {
        return $"❌ Database error: {ex.Message}";
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