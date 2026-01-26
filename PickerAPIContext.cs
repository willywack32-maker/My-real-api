using Microsoft.EntityFrameworkCore;
using TheRocksNew.API.Models;

namespace TheRocksNew.API.Data
{
    public class PickerAPIContext : DbContext
    {
        public PickerAPIContext(DbContextOptions<PickerAPIContext> options) : base(options)
        {
        }
        
        public DbSet<Picker> Pickers { get; set; }
        public DbSet<Orchard> Orchards { get; set; }
        public DbSet<OrchardBlock> OrchardBlocks { get; set; }
        public DbSet<ApplePrice> ApplePrices { get; set; }
        public DbSet<PickRecord> PickRecords { get; set; }
        public DbSet<Packhouse> Packhouses { get; set; }
        
        // REMOVE OR COMMENT OUT THE OnModelCreating METHOD
        // Since we don't have navigation properties, we don't need this
    }
}