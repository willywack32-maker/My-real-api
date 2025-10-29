using Microsoft.EntityFrameworkCore;

public class PickeAPIContext : DbContext
{
    public PickeAPIContext(DbContextOptions<PickeAPIContext> options) : base(options) { }
    
    public DbSet<Picker> Pickers { get; set; }
}

public class Picker
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string OrchardName { get; set; }
    public string PackHouse { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal BinRate { get; set; }
}