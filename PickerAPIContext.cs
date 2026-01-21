using Microsoft.EntityFrameworkCore;
using TheRocksNew.API.Models;

namespace TheRocksNew.API.Data
{
    public class PickerAPIContext : DbContext
    {
        public PickerAPIContext(DbContextOptions<PickerAPIContext> options) : base(options)
        {
        }
        
        // Make sure ALL these DbSets exist:
        public DbSet<Picker> Pickers { get; set; }
        public DbSet<Orchard> Orchards { get; set; }
        public DbSet<OrchardBlock> OrchardBlocks { get; set; }
        public DbSet<ApplePrice> ApplePrices { get; set; }
        public DbSet<PickRecord> PickRecords { get; set; }
        public DbSet<Packhouse> Packhouses { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<OrchardBlock>()
                .HasOne(ob => ob.Orchard)
                .WithMany()
                .HasForeignKey(ob => ob.OrchardId);
                
            modelBuilder.Entity<PickRecord>()
                .HasOne(pr => pr.Picker)
                .WithMany()
                .HasForeignKey(pr => pr.PickerId);
                
            modelBuilder.Entity<PickRecord>()
                .HasOne(pr => pr.OrchardBlock)
                .WithMany()
                .HasForeignKey(pr => pr.OrchardBlockId);
        }
    }
}