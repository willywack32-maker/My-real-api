using Microsoft.EntityFrameworkCore;
using TheRocksNew.API.Data;
using TheRocksNew.API.Models;
using Npgsql;

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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Picker API v1");
    c.RoutePrefix = "swagger"; // Set Swagger at /swagger
});
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
            // Test a simple query
            var pickerCount = await dbContext.Pickers.CountAsync();
            logger.LogInformation($"📊 Found {pickerCount} pickers in database");
            
            var created = await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation($"📊 Database tables: {(created ? "✅ CREATED" : "✅ ALREADY EXIST")}");
        }
        else
        {
            logger.LogError("❌ Cannot connect to database. Check connection string and network settings.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ DATABASE SETUP FAILED");
    }



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

app.MapGet("/api/debug/connection", () =>
{
    var connString = Environment.GetEnvironmentVariable("DATABASE_URL");
    var hasConnString = !string.IsNullOrEmpty(connectionString);
    
    // Mask password for display
    string masked = "Not set";
    if (!string.IsNullOrEmpty(connString))
    {
        try
        {
            var uri = new Uri(connString);
            var userInfo = uri.UserInfo;
            if (userInfo.Contains(':'))
            {
                var parts = userInfo.Split(':');
                masked = connString.Replace(userInfo, $"{parts[0]}:***");
            }
        }
        catch
        {
            masked = connString.Length > 50 ? connString.Substring(0, 50) + "..." : connString;
        }
    }
    
    return new
    {
        DATABASE_URL_Set = hasConnString,
        ConnectionString = masked,
        Environment = app.Environment.EnvironmentName,
        ServerTime = DateTime.UtcNow
    };
});

app.MapGet("/api/debug/tables", async (PickerAPIContext context) =>
{
    try
    {
        var pickers = await context.Pickers.CountAsync();
        var orchards = await context.Orchards.CountAsync();
        var blocks = await context.OrchardBlocks.CountAsync();
        var prices = await context.ApplePrices.CountAsync();
        var records = await context.PickRecords.CountAsync();
        var packhouses = await context.Packhouses.CountAsync();
        
        return Results.Ok(new
        {
            Pickers = pickers,
            Orchards = orchards,
            OrchardBlocks = blocks,
            ApplePrices = prices,
            PickRecords = records,
            Packhouses = packhouses,
            TotalTables = 6
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
});

app.Run();

static string? ConvertSupabaseConnectionString(string? supabaseUrl)
{
    try
    {
        Console.WriteLine($"🔍 Raw connection string: {supabaseUrl?.Substring(0, Math.Min(supabaseUrl.Length, 100))}...");
        
        if (string.IsNullOrEmpty(supabaseUrl))
        {
            Console.WriteLine("❌ Connection string is null or empty");
            return null;
        }
        
        // Handle both pooler and direct connection strings
        if (supabaseUrl.StartsWith("postgresql://") || supabaseUrl.StartsWith("postgres://"))
        {
            var uri = new Uri(supabaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            
            if (userInfo.Length < 2)
            {
                Console.WriteLine($"❌ Invalid user info format: {uri.UserInfo}");
                return null;
            }
            
            var username = userInfo[0];
            var password = Uri.UnescapeDataString(userInfo[1]);
            
            Console.WriteLine($"✅ Parsed - Host: {uri.Host}, Port: {uri.Port}, User: {username}, DB: {uri.LocalPath.TrimStart('/')}");
            
            // Build connection string
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Database = uri.LocalPath.TrimStart('/'),
                Username = username,
                Password = password,
                SslMode = SslMode.Require,
                Pooling = true,
                MinPoolSize = 0,
                MaxPoolSize = 20,
                Timeout = 30,
                CommandTimeout = 30,
                TcpKeepAlive = true,
                KeepAlive = 60
            };
            
            var result = builder.ToString();
            Console.WriteLine($"✅ Converted successfully");
            return result;
        }
        
        // Already in Npgsql format
        Console.WriteLine("ℹ️ Already in Npgsql format");
        return supabaseUrl;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Conversion failed: {ex.Message}");
        Console.WriteLine($"📋 Stack: {ex.StackTrace}");
        return null;
    }
}