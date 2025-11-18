using Microsoft.EntityFrameworkCore;
using TheRocksNew.API.Models;

namespace TheRocksNew.API.Data;

{
    public class PickerAPIContext : DbContext
    {
        public PickerAPIContext(DbContextOptions<PickerAPIContext> options) : base(options) { }

        public DbSet<ViewModels.Picker> Pickers { get; set; }
        public DbSet<Orchard> Orchards { get; set; }
        public DbSet<OrchardBlock> OrchardBlocks { get; set; }
        public DbSet<ApplePrice> ApplePrices { get; set; }
        public DbSet<Packhouse> Packhouses { get; set; }
        public DbSet<PickRecord> PickRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // REMOVE ALL SQL Server specific code
            // Just use base implementation or PostgreSQL compatible code
            base.OnModelCreating(modelBuilder);

            // If you need to configure relationships, do it like this:
            modelBuilder.Entity<OrchardBlock>()
                .HasOne<Orchard>()
                .WithMany()
                .HasForeignKey(b => b.OrchardId);
        }
    }
}