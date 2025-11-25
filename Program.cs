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

// Database configuration
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(connectionString))
{
    // Convert Supabase connection string to standard format
    connectionString = ConvertSupabaseConnectionString(connectionString);
    Console.WriteLine("✅ Using Supabase database (converted)");
}
else
{
    // Fallback to appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine("⚠️ Using fallback connection string");
}

Console.WriteLine($"🔌 Database connection: {!string.IsNullOrEmpty(connectionString)}");

builder.Services.AddDbContext<PickerAPIContext>(options =>
    options.UseNpgsql(connectionString, 
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5)
        )));

var app = builder.Build();

// ✅ FIXED: Proper async database initialization
async Task InitializeDatabaseAsync()
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🆕 ATTEMPTING DATABASE CONNECTION...");
        var dbContext = services.GetRequiredService<PickerAPIContext>();
        
        // Simple connection test first
        var canConnect = await dbContext.Database.CanConnectAsync();
        logger.LogInformation($"🔌 Database connection: {(canConnect ? "✅ SUCCESS" : "❌ FAILED")}");
        
        if (canConnect)
        {
            // Then create tables if needed
            var created = await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation($"📊 Database tables: {(created ? "✅ CREATED" : "✅ ALREADY EXIST")}");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ DATABASE SETUP FAILED");
        // Don't crash the app, just log the error
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
            FirstName = "Test",
            LastName = "Picker",
            Email = "test@example.com",
            Phone = "123-456-7890",
            IsActive = true
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
app.MapGet("/api/pickers/active", async (PickerAPIContext context) => 
    await context.Pickers.Where(p => p.IsActive).ToListAsync());

app.MapGet("/api/orchards/active", async (PickerAPIContext context) => 
    await context.Orchards.Where(o => o.IsActive).ToListAsync());

app.MapGet("/api/orchard-blocks/active", async (PickerAPIContext context) => 
    await context.OrchardBlocks
        .Where(b => b.IsActive)
        .ToListAsync());

app.MapGet("/api/apple-varieties/available", async (PickerAPIContext context) => 
    await context.OrchardBlocks
        .Where(b => b.IsActive && !string.IsNullOrEmpty(b.AppleVariety))
        .Select(b => b.AppleVariety)
        .Distinct()
        .ToListAsync());

app.MapGet("/api/bin-rates/current", async (PickerAPIContext context) => 
    await context.ApplePrices
        .Where(p => p.IsActive)
        .Select(p => new { p.Variety, p.BinRate })
        .ToListAsync());

// GET bin rate for a specific variety
app.MapGet("/api/bin-rates/{variety}", async (PickerAPIContext context, string variety) =>
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
app.MapPost("/api/picks", async (PickerAPIContext context, PickRecord pick) =>
{
    pick.Id = Guid.NewGuid();
    pick.PickDate = DateTime.UtcNow;
    
    context.PickRecords.Add(pick);
    await context.SaveChangesAsync();
    
    // Return the pick without navigation properties since they were removed
    return Results.Created($"/api/picks/{pick.Id}", pick);
});

// ADMIN ENDPOINTS - View all picks
app.MapGet("/api/admin/pick-records", async (PickerAPIContext context) => 
    await context.PickRecords
        .OrderByDescending(p => p.PickDate)
        .ToListAsync());

// ADMIN: Get picker earnings summary
app.MapGet("/api/admin/picker-earnings", async (PickerAPIContext context) =>
{
    var earnings = await context.PickRecords
        .GroupBy(p => p.PickerId)
        .Select(g => new
        {
            PickerId = g.Key,
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

// Helper method to convert Supabase connection string - MUST be at the end
static string ConvertSupabaseConnectionString(string supabaseUrl)
{
    try
    {
        var uri = new Uri(supabaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        
        return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
    }
    catch (Exception ex)
    {
        throw new ArgumentException($"Invalid Supabase connection string: {supabaseUrl}", ex);
    }
}