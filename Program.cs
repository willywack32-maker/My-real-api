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

// MAIN PAGE ENDPOINTS - for dropdowns and submitting picks
app.MapGet("/api/pickers/active", async (AppDbContext context) => 
    await context.Pickers.Where(p => p.IsActive).ToListAsync());

app.MapGet("/api/orchards/active", async (AppDbContext context) => 
    await context.Orchards.Where(o => o.IsActive).ToListAsync());

app.MapGet("/api/orchard-blocks/active", async (AppDbContext context) => 
    await context.OrchardBlocks
        .Include(b => b.Orchard)
        .Where(b => b.IsActive)
        .ToListAsync());

app.MapGet("/api/apple-varieties/available", async (AppDbContext context) => 
    await context.OrchardBlocks
        .Where(b => b.IsActive && !string.IsNullOrEmpty(b.AppleVariety))
        .Select(b => b.AppleVariety)
        .Distinct()
        .ToListAsync());

app.MapGet("/api/bin-rates/current", async (AppDbContext context) => 
    await context.ApplePrices
        .Where(p => p.IsActive)
        .Select(p => new { p.Variety, p.BinRate })
        .ToListAsync());

// GET bin rate for a specific variety
app.MapGet("/api/bin-rates/{variety}", async (AppDbContext context, string variety) =>
{
    var price = await context.ApplePrices
        .FirstOrDefaultAsync(p => p.Variety == variety && p.IsActive);
    
    if (price != null)
    {
        return Results.Ok(price.BinRate);
    }
    
    // Fallback to block's default rate
    var block = await context.OrchardBlocks
        .FirstOrDefaultAsync(b => b.AppleVariety == variety && b.IsActive);
    
    return Results.Ok(block?.DefaultBinRate ?? 45.00m);
});

// SUBMIT PICK FROM MAIN PAGE
app.MapPost("/api/picks", async (AppDbContext context, PickRecord pick) =>
{
    pick.Id = Guid.NewGuid();
    pick.PickDate = DateTime.UtcNow;
    
    context.PickRecords.Add(pick);
    await context.SaveChangesAsync();
    
    // Return the pick with calculated total
    var result = await context.PickRecords
        .Include(p => p.Picker)
        .Include(p => p.OrchardBlock)
        .ThenInclude(b => b.Orchard)
        .FirstOrDefaultAsync(p => p.Id == pick.Id);
        
    return Results.Created($"/api/picks/{pick.Id}", result);
});

// ADMIN ENDPOINTS - View all picks
app.MapGet("/api/admin/pick-records", async (AppDbContext context) => 
    await context.PickRecords
        .Include(p => p.Picker)
        .Include(p => p.OrchardBlock)
        .ThenInclude(b => b.Orchard)
        .OrderByDescending(p => p.PickDate)
        .ToListAsync());

// ADMIN: Get picker earnings summary
app.MapGet("/api/admin/picker-earnings", async (AppDbContext context) =>
{
    var earnings = await context.PickRecords
        .Include(p => p.Picker)
        .GroupBy(p => new { p.PickerId, p.Picker.FirstName, p.Picker.LastName })
        .Select(g => new
        {
            PickerName = $"{g.Key.FirstName} {g.Key.LastName}",
            TotalBins = g.Sum(p => p.BinsPicked),
            TotalEarnings = g.Sum(p => p.TotalAmount),
            AverageBinRate = g.Average(p => p.BinRate)
        })
        .ToListAsync();
        
    return Results.Ok(earnings);
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