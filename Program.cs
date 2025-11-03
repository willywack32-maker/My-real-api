using Microsoft.EntityFrameworkCore;
using TheRocksNew.API.Data;    // For DbContext
using TheRocksNew.API.Models;  // For Model

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ ADD CORS - CRITICAL FOR MAUI APP
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMauiApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("🚀 DATABASE SETUP: Starting application...");
Console.WriteLine($"🔌 Connection string configured: {!string.IsNullOrEmpty(connectionString)}");

builder.Services.AddDbContext<PickerAPIContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// ✅ FIXED: Proper async database initialization
async Task InitializeDatabaseAsync()
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🆕 CREATING DATABASE TABLES...");
        var dbContext = services.GetRequiredService<PickerAPIContext>();
        
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

// ✅ Initialize database synchronously for startup
await InitializeDatabaseAsync();

// ✅ USE CORS - MUST come before other middleware
app.UseCors("AllowMauiApp");

// Test endpoints
app.MapGet("/", () => "API Root - Working!");
app.MapGet("/test", () => "Test endpoint - Working!");

app.MapGet("/db-test", async (PickerAPIContext dbContext) => 
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

app.MapGet("/create-test", async (PickerAPIContext dbContext) => 
{
    try 
    {
        var testPicker = new Picker 
        { 
            Name = "Test Picker " + DateTime.Now.Ticks, 
            OrchardName = "Test Orchard",
            PackHouse = "Test House",
            HoursWorked = 8.0m,
            BinRate = 15.5m
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