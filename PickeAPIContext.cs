using Microsoft.EntityFrameworkCore;

namespace TheRocksNew.API.Data
{
    public class PickerAPIContext : DbContext
    {
        public PickerAPIContext(DbContextOptions<PickerAPIContext> options)
            : base(options)
        {
        }

        public DbSet<SharedModels.Picker> Pickers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SharedModels.Picker>()
                .Property(p => p.HoursWorked)
                .HasColumnType("decimal(10,4)"); // e.g., 1234.5678 hours
            modelBuilder.Entity<SharedModels.Picker>()
                .Property(p => p.BinRate)
                .HasColumnType("decimal(10,2)"); // e.g., 123456.78
        }
    }
}