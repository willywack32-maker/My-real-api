using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database (using in-memory for now)
builder.Services.AddDbContext<PickeAPIContext>(options =>
    options.UseInMemoryDatabase("PickerDB"));

var app = builder.Build();

// Add test endpoints FIRST
app.MapGet("/", () => "API Root - Working!");
app.MapGet("/test", () => "Test endpoint - Working!");
app.MapGet("/api/test", () => "API Test - Working!");

// Then map controllers
app.MapControllers();

// Development features
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.Run();