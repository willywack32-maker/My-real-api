using Microsoft.EntityFrameworkCore;
using TheRocksNew.API.Models;

namespace TheRocksNew.API.Data;

public class PickerAPIContext : DbContext
{
    public PickerAPIContext(DbContextOptions<PickerAPIContext> options) : base(options) { }
    
    public DbSet<Picker> Pickers { get; set; } = null!;
}

