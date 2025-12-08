using Microsoft.EntityFrameworkCore;
using TheRocksNew.API.Data;
using TheRocksNew.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMauiApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(connectionString))
{
    connectionString = ConvertSupabaseConnectionString(connectionString);
    Console.WriteLine("✅ Using Supabase database (converted)");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine("⚠️ Using fallback connection string");
}

Console.WriteLine($"🔌 Database connection: {!string.IsNullOrEmpty(connectionString)}");

builder.Services.AddDbContext<PickerAPIContext>(options =>
    options.UseNpgsql(connectionString));
    
var app = builder.Build();

app.UseCors("AllowMauiApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

async Task InitializeDatabaseAsync()
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🆕 ATTEMPTING DATABASE CONNECTION...");
        var dbContext = services.GetRequiredService<PickerAPIContext>();
        
        var canConnect = await dbContext.Database.CanConnectAsync();
        logger.LogInformation($"🔌 Database connection: {(canConnect ? "✅ SUCCESS" : "❌ FAILED")}");
        
        if (canConnect)
        {
            var created = await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation($"📊 Database tables: {(created ? "✅ CREATED" : "✅ ALREADY EXIST")}");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ DATABASE SETUP FAILED");
    }
}

await InitializeDatabaseAsync();

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

// COMMENTED OUT - FIX LATER
// app.MapGet("/create-test", async (PickerAPIContext dbContext) => 
// {
//     return "This endpoint is temporarily disabled";
// });

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

app.MapGet("/api/bin-rates/{variety}", async (PickerAPIContext context, string variety) =>
{
    var price = await context.ApplePrices
        .FirstOrDefaultAsync(p => p.Variety == variety && p.IsActive);
    
    if (price != null)
    {
        return Results.Ok(price.BinRate);
    }
    
    var block = await context.OrchardBlocks
        .FirstOrDefaultAsync(b => b.AppleVariety == variety && b.IsActive);
    
    return Results.Ok(block?.DefaultBinRate ?? 45.00m);
});

app.MapPost("/api/picks", async (PickerAPIContext context, PickRecord pick) =>
{
    pick.Id = Guid.NewGuid();
    pick.PickDate = DateTime.UtcNow;
    
    context.PickRecords.Add(pick);
    await context.SaveChangesAsync();
    
    return Results.Created($"/api/picks/{pick.Id}", pick);
});

app.MapGet("/api/admin/pick-records", async (PickerAPIContext context) => 
    await context.PickRecords
        .OrderByDescending(p => p.PickDate)
        .ToListAsync());

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

app.Run();

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