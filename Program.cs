using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL
builder.Services.AddDbContext<PickeAPIContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ADD THESE TEST ENDPOINTS:
app.MapGet("/", () => "My-Real-API is running!");
app.MapGet("/test", () => "Test endpoint works!");
app.MapGet("/api/test", () => "API test works!");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();