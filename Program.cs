using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("🚀 DATABASE SETUP: Starting application...");
Console.WriteLine($"🔌 Connection string configured: {!string.IsNullOrEmpty(connectionString)}");

builder.Services.AddDbContext<PickeAPIContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// 🆕 NEW DATABASE CREATION CODE - REPLACES MIGRATIONS
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🆕 CREATING DATABASE TABLES...");
        var dbContext = services.GetRequiredService<PickeAPIContext>();
        
        // This will create the database and tables if they don't exist
        var created = await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation($"✅ DATABASE CREATION: {(created ? "TABLES CREATED" : "TABLES ALREADY EXIST")}");
        
        // Test the Picker table
        var pickerCount = await dbContext.Pickers.CountAsync();
        logger.LogInformation($"📊 Picker table test: {pickerCount} records found");
        
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ DATABASE CREATION FAILED");
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
        return $"Database connection: {(canConnect ? "✅ WORKING" : "❌ FAILED")}";
    }
    catch (Exception ex)
    {
        return $"Database error: {ex.Message}";
    }
});

app.MapGet("/create-test", async (PickeAPIContext dbContext) => 
{
    try 
    {
        var testPicker = new Picker 
        { 
            Name = "Test Picker " + DateTime.Now.Ticks, 
            OrchardName = "Test Orchard",
            PackHouse = "Test House",
            HoursWorked = 8.0m,  // ✅ FIXED: Using decimal value instead of string
            BinRate = 15.5m      // ✅ FIXED: Using decimal value instead of string
        };
        
        dbContext.Pickers.Add(testPicker);
        await dbContext.SaveChangesAsync();
        
        var count = await dbContext.Pickers.CountAsync();
        return $"✅ SUCCESS! Created test picker. Total pickers: {count}";
    }
    catch (Exception ex)
    {
        return $"❌ FAILED: {ex.Message}";
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