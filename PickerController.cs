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
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Picker API is working!");
        }

        // =========== PICKER ENDPOINTS ===========
        
        [HttpGet("admin/all")]
        public async Task<ActionResult<IEnumerable<Picker>>> GetAllPickers()
        {
            return await _context.Pickers.ToListAsync();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Picker>>> GetActivePickers()
        {
            return await _context.Pickers
                .Where(p => p.IsActive)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }

        [HttpPost("admin/create")]
        public async Task<ActionResult<Picker>> CreatePicker(Picker picker)
        {
            picker.Id = Guid.NewGuid();
            picker.IsActive = true;
            picker.HireDate = DateTime.Today;
            
            _context.Pickers.Add(picker);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetAllPickers), new { id = picker.Id }, picker);
        }

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
        
        [HttpGet("orchards")]
        public async Task<ActionResult<IEnumerable<Orchard>>> GetOrchards()
        {
            return await _context.Orchards.ToListAsync();
        }

        [HttpGet("orchards/active")]
        public async Task<ActionResult<IEnumerable<Orchard>>> GetActiveOrchards()
        {
            return await _context.Orchards
                .Where(o => o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

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
        
        [HttpGet("orchardblocks")]
        public async Task<ActionResult<IEnumerable<OrchardBlock>>> GetOrchardBlocks()
        {
            return await _context.OrchardBlocks.ToListAsync(); // REMOVED .Include
        }

        [HttpGet("orchardblocks/active")]
        public async Task<ActionResult<IEnumerable<OrchardBlock>>> GetActiveOrchardBlocks()
        {
            return await _context.OrchardBlocks
                .Where(ob => ob.IsActive)
                .OrderBy(ob => ob.BlockName)
                .ThenBy(ob => ob.BlockNumber)
                .ToListAsync(); // REMOVED .Include
        }

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
        
        [HttpGet("appleprices")]
        public async Task<ActionResult<IEnumerable<ApplePrice>>> GetApplePrices()
        {
            return await _context.ApplePrices.ToListAsync();
        }

        [HttpGet("appleprices/active")]
        public async Task<ActionResult<IEnumerable<ApplePrice>>> GetActiveApplePrices()
        {
            return await _context.ApplePrices
                .Where(ap => ap.IsActive)
                .OrderBy(ap => ap.Variety)
                .ToListAsync();
        }

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

        [HttpGet("binrate/{variety}")]
        public async Task<ActionResult<decimal>> GetBinRate(string variety)
        {
            var applePrice = await _context.ApplePrices
                .Where(ap => ap.Variety == variety && ap.IsActive)
                .OrderByDescending(ap => ap.EffectiveDate)
                .FirstOrDefaultAsync();
            
            if (applePrice == null)
                return Ok(45.00m);
            
            return Ok(applePrice.BinRate);
        }

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
        
        [HttpGet("pickrecords")]
        public async Task<ActionResult<IEnumerable<PickRecord>>> GetPickRecords()
        {
            return await _context.PickRecords
                .OrderByDescending(pr => pr.PickDate)
                .ToListAsync(); // REMOVED .Include
        }

        [HttpGet("pickrecords/admin")]
        public async Task<ActionResult<IEnumerable<PickRecord>>> GetAdminPickRecords()
        {
            return await _context.PickRecords
                .OrderByDescending(pr => pr.PickDate)
                .ToListAsync(); // REMOVED .Include
        }

        [HttpPost("pickrecords")]
        public async Task<ActionResult<PickRecord>> CreatePickRecord(PickRecord pickRecord)
        {
            pickRecord.Id = Guid.NewGuid();
            pickRecord.PickDate = pickRecord.PickDate.Date;
            
            _context.PickRecords.Add(pickRecord);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetPickRecords), new { id = pickRecord.Id }, pickRecord);
        }

        // =========== PACKHOUSE ENDPOINTS ===========
        
        [HttpGet("packhouses")]
        public async Task<ActionResult<IEnumerable<Packhouse>>> GetPackhouses()
        {
            return await _context.Packhouses.ToListAsync();
        }

        [HttpGet("packhouses/active")]
        public async Task<ActionResult<IEnumerable<Packhouse>>> GetActivePackhouses()
        {
            return await _context.Packhouses
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        [HttpPost("packhouses")]
        public async Task<ActionResult<Packhouse>> CreatePackhouse(Packhouse packhouse)
        {
            packhouse.Id = Guid.NewGuid();
            packhouse.IsActive = true;
            
            _context.Packhouses.Add(packhouse);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetPackhouses), new { id = packhouse.Id }, packhouse);
        }

        // =========== SEED DATABASE ===========
        
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