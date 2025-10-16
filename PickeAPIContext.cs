using Microsoft.EntityFrameworkCore;

public class PickeAPIContext : DbContext
{
    public PickeAPIContext(DbContextOptions<PickeAPIContext> options) : base(options)
    {
    }

    public DbSet<Picker> Pickers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // This will be set via dependency injection
    }
}