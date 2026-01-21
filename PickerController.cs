using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheRocksNew.API.Data;
using TheRocksNew.API.Models;

namespace TheRocksNew.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PickerController : ControllerBase
    {
        private readonly PickerAPIContext _context;

        public PickerController(PickerAPIContext context)
        {
            _context = context;
        }

        // =========== HEALTH CHECK ===========
        // GET: api/picker
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Picker API is working!");
        }

        // =========== PICKER ENDPOINTS ===========
        
        // GET: api/picker/admin/all - Get all pickers (admin view)
        [HttpGet("admin/all")]
        public async Task<ActionResult<IEnumerable<Picker>>> GetAllPickers()
        {
            return await _context.Pickers.ToListAsync();
        }

        // GET: api/picker/active - Get active pickers (for dropdown)
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Picker>>> GetActivePickers()
        {
            return await _context.Pickers
                .Where(p => p.IsActive)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }

        // POST: api/picker/admin/create - Create new picker
        [HttpPost("admin/create")]
        public async Task<ActionResult<Picker>> CreatePicker(Picker picker)
        {
            // Set default values
            picker.Id = Guid.NewGuid();
            picker.IsActive = true;
            picker.HireDate = DateTime.Today;
            
            _context.Pickers.Add(picker);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetAllPickers), new { id = picker.Id }, picker);
        }

        // PUT: api/picker/admin/{id}/status - Activate/deactivate picker
        [HttpPut("admin/{id}/status")]
        public async Task<IActionResult> UpdatePickerStatus(Guid id, [FromBody] bool isActive)
        {
            var picker = await _context.Pickers.FindAsync(id);
            if (picker == null)
                return NotFound();
            
            picker.IsActive = isActive;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // =========== ORCHARD ENDPOINTS ===========
        
        // GET: api/picker/orchards - Get all orchards
        [HttpGet("orchards")]
        public async Task<ActionResult<IEnumerable<Orchard>>> GetOrchards()
        {
            return await _context.Orchards.ToListAsync();
        }

        // GET: api/picker/orchards/active - Get active orchards (for dropdown)
        [HttpGet("orchards/active")]
        public async Task<ActionResult<IEnumerable<Orchard>>> GetActiveOrchards()
        {
            return await _context.Orchards
                .Where(o => o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        // POST: api/picker/orchards - Create new orchard
        [HttpPost("orchards")]
        public async Task<ActionResult<Orchard>> CreateOrchard(Orchard orchard)
        {
            orchard.Id = Guid.NewGuid();
            orchard.IsActive = true;
            
            _context.Orchards.Add(orchard);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetOrchards), new { id = orchard.Id }, orchard);
        }

        // =========== ORCHARD BLOCK ENDPOINTS ===========
        
        // GET: api/picker/orchardblocks - Get all orchard blocks
        [HttpGet("orchardblocks")]
        public async Task<ActionResult<IEnumerable<OrchardBlock>>> GetOrchardBlocks()
        {
            return await _context.OrchardBlocks
                .Include(ob => ob.Orchard)
                .ToListAsync();
        }

        // GET: api/picker/orchardblocks/active - Get active orchard blocks
        [HttpGet("orchardblocks/active")]
        public async Task<ActionResult<IEnumerable<OrchardBlock>>> GetActiveOrchardBlocks()
        {
            return await _context.OrchardBlocks
                .Where(ob => ob.IsActive)
                .Include(ob => ob.Orchard)
                .OrderBy(ob => ob.BlockName)
                .ThenBy(ob => ob.BlockNumber)
                .ToListAsync();
        }

        // POST: api/picker/orchardblocks - Create new orchard block
        [HttpPost("orchardblocks")]
        public async Task<ActionResult<OrchardBlock>> CreateOrchardBlock(OrchardBlock orchardBlock)
        {
            orchardBlock.Id = Guid.NewGuid();
            orchardBlock.IsActive = true;
            
            _context.OrchardBlocks.Add(orchardBlock);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetOrchardBlocks), new { id = orchardBlock.Id }, orchardBlock);
        }

        // =========== APPLE PRICE/VARIETY ENDPOINTS ===========
        
        // GET: api/picker/appleprices - Get all apple prices
        [HttpGet("appleprices")]
        public async Task<ActionResult<IEnumerable<ApplePrice>>> GetApplePrices()
        {
            return await _context.ApplePrices.ToListAsync();
        }

        // GET: api/picker/appleprices/active - Get active apple prices
        [HttpGet("appleprices/active")]
        public async Task<ActionResult<IEnumerable<ApplePrice>>> GetActiveApplePrices()
        {
            return await _context.ApplePrices
                .Where(ap => ap.IsActive)
                .OrderBy(ap => ap.Variety)
                .ToListAsync();
        }

        // GET: api/picker/applevarieties - Get distinct apple varieties
        [HttpGet("applevarieties")]
        public async Task<ActionResult<IEnumerable<string>>> GetAppleVarieties()
        {
            return await _context.ApplePrices
                .Where(ap => ap.IsActive)
                .Select(ap => ap.Variety)
                .Distinct()
                .OrderBy(v => v)
                .ToListAsync();
        }

        // GET: api/picker/binrate/{variety} - Get bin rate for specific variety
        [HttpGet("binrate/{variety}")]
        public async Task<ActionResult<decimal>> GetBinRate(string variety)
        {
            var applePrice = await _context.ApplePrices
                .Where(ap => ap.Variety == variety && ap.IsActive)
                .OrderByDescending(ap => ap.EffectiveDate)
                .FirstOrDefaultAsync();
            
            if (applePrice == null)
                return Ok(45.00m); // Default rate
            
            return Ok(applePrice.BinRate);
        }

        // POST: api/picker/appleprices - Create new apple price
        [HttpPost("appleprices")]
        public async Task<ActionResult<ApplePrice>> CreateApplePrice(ApplePrice applePrice)
        {
            applePrice.Id = Guid.NewGuid();
            applePrice.IsActive = true;
            
            _context.ApplePrices.Add(applePrice);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetApplePrices), new { id = applePrice.Id }, applePrice);
        }

        // =========== PICK RECORD ENDPOINTS ===========
        
        // GET: api/picker/pickrecords - Get all pick records
        [HttpGet("pickrecords")]
        public async Task<ActionResult<IEnumerable<PickRecord>>> GetPickRecords()
        {
            return await _context.PickRecords
                .Include(pr => pr.Picker)
                .Include(pr => pr.OrchardBlock)
                .ThenInclude(ob => ob.Orchard)
                .OrderByDescending(pr => pr.PickDate)
                .ToListAsync();
        }

        // GET: api/picker/pickrecords/admin - Get all pick records (admin view)
        [HttpGet("pickrecords/admin")]
        public async Task<ActionResult<IEnumerable<PickRecord>>> GetAdminPickRecords()
        {
            return await _context.PickRecords
                .Include(pr => pr.Picker)
                .Include(pr => pr.OrchardBlock)
                .ThenInclude(ob => ob.Orchard)
                .OrderByDescending(pr => pr.PickDate)
                .ToListAsync();
        }

        // POST: api/picker/pickrecords - Create new pick record
        [HttpPost("pickrecords")]
        public async Task<ActionResult<PickRecord>> CreatePickRecord(PickRecord pickRecord)
        {
            pickRecord.Id = Guid.NewGuid();
            pickRecord.PickDate = pickRecord.PickDate.Date; // Ensure date only
            
            _context.PickRecords.Add(pickRecord);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetPickRecords), new { id = pickRecord.Id }, pickRecord);
        }

        // =========== PACKHOUSE ENDPOINTS ===========
        
        // GET: api/picker/packhouses - Get all packhouses
        [HttpGet("packhouses")]
        public async Task<ActionResult<IEnumerable<Packhouse>>> GetPackhouses()
        {
            return await _context.Packhouses.ToListAsync();
        }

        // GET: api/picker/packhouses/active - Get active packhouses
        [HttpGet("packhouses/active")]
        public async Task<ActionResult<IEnumerable<Packhouse>>> GetActivePackhouses()
        {
            return await _context.Packhouses
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        // POST: api/picker/packhouses - Create new packhouse
        [HttpPost("packhouses")]
        public async Task<ActionResult<Packhouse>> CreatePackhouse(Packhouse packhouse)
        {
            packhouse.Id = Guid.NewGuid();
            packhouse.IsActive = true;
            
            _context.Packhouses.Add(packhouse);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetPackhouses), new { id = packhouse.Id }, packhouse);
        }

        // =========== TEST DATA ENDPOINT ===========
        
        // POST: api/picker/seed - Seed database with test data
        [HttpPost("seed")]
        public async Task<IActionResult> SeedDatabase()
        {
            // Clear existing data
            _context.Pickers.RemoveRange(_context.Pickers);
            _context.Orchards.RemoveRange(_context.Orchards);
            _context.OrchardBlocks.RemoveRange(_context.OrchardBlocks);
            _context.ApplePrices.RemoveRange(_context.ApplePrices);
            _context.PickRecords.RemoveRange(_context.PickRecords);
            _context.Packhouses.RemoveRange(_context.Packhouses);
            
            await _context.SaveChangesAsync();
            
            // Add test pickers
            var pickers = new List<Picker>
            {
                new Picker { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "555-0101", IsActive = true, HireDate = DateTime.Today.AddMonths(-6) },
                new Picker { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Phone = "555-0102", IsActive = true, HireDate = DateTime.Today.AddMonths(-3) },
                new Picker { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Johnson", Email = "bob@example.com", Phone = "555-0103", IsActive = true, HireDate = DateTime.Today.AddMonths(-1) }
            };
            _context.Pickers.AddRange(pickers);
            
            // Add test orchards
            var orchards = new List<Orchard>
            {
                new Orchard { Id = Guid.NewGuid(), Name = "Sunrise Orchard", Location = "North Valley", TotalArea = 50.5m, IsActive = true },
                new Orchard { Id = Guid.NewGuid(), Name = "Valley View", Location = "South Hills", TotalArea = 75.2m, IsActive = true },
                new Orchard { Id = Guid.NewGuid(), Name = "Golden Apple", Location = "West Ridge", TotalArea = 40.8m, IsActive = true }
            };
            _context.Orchards.AddRange(orchards);
            
            // Add test apple prices
            var applePrices = new List<ApplePrice>
            {
                new ApplePrice { Id = Guid.NewGuid(), Variety = "Gala", PricePerKg = 3.50m, BinRate = 45.00m, EffectiveDate = DateTime.Today, IsActive = true },
                new ApplePrice { Id = Guid.NewGuid(), Variety = "Fuji", PricePerKg = 3.75m, BinRate = 48.00m, EffectiveDate = DateTime.Today, IsActive = true },
                new ApplePrice { Id = Guid.NewGuid(), Variety = "Honeycrisp", PricePerKg = 4.25m, BinRate = 52.00m, EffectiveDate = DateTime.Today, IsActive = true },
                new ApplePrice { Id = Guid.NewGuid(), Variety = "Granny Smith", PricePerKg = 3.25m, BinRate = 42.00m, EffectiveDate = DateTime.Today, IsActive = true }
            };
            _context.ApplePrices.AddRange(applePrices);
            
            await _context.SaveChangesAsync();
            
            return Ok("Database seeded with test data!");
        }
    }
}