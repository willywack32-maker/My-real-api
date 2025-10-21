using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Convert PostgreSQL URL to standard connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString.StartsWith("postgresql://"))
{
    // Convert from: postgresql://user:password@host:port/database
    // To: Host=host;Port=port;Database=database;Username=user;Password=password
    var uri = new Uri(connectionString);
    var db = uri.AbsolutePath.Trim('/');
    var user = uri.UserInfo.Split(':')[0];
    var passwd = uri.UserInfo.Split(':')[1];
    
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={db};Username={user};Password={passwd};SSL Mode=Require;Trust Server Certificate=true";
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL
builder.Services.AddDbContext<PickeAPIContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Add this - automatically run migrations on startup
//using (var scope = app.Services.CreateScope())
//{
  //  var db = scope.ServiceProvider.GetRequiredService<PickeAPIContext>();
    //db.Database.Migrate();
//}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add these test endpoints:
app.MapGet("/", () => "Root works!");
app.MapGet("/test", () => "Test works!"); 
app.MapGet("/api/test", () => "API test works!");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();