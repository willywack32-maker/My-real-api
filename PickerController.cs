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
        public IActionResult Get() => Ok("Picker API is working!");

        // =========== PICKERS ===========
        [HttpGet("admin/all")]
        public async Task<ActionResult<IEnumerable<Picker>>> GetAllPickers() => await _context.Pickers.ToListAsync();

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Picker>>> GetActivePickers() =>
            await _context.Pickers.Where(p => p.IsActive).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync();

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

        [HttpPut("admin/{id}")]
        public async Task<IActionResult> UpdatePicker(Guid id, Picker updated)
        {
            var p = await _context.Pickers.FindAsync(id);
            if (p == null) return NotFound();
            p.FirstName = updated.FirstName;
            p.LastName = updated.LastName;
            p.Email = updated.Email;
            p.Phone = updated.Phone;
            p.IsActive = updated.IsActive;
            await _context.SaveChangesAsync();
            return Ok(p);
        }

        [HttpPut("admin/{id}/status")]
        public async Task<IActionResult> UpdatePickerStatus(Guid id, [FromBody] bool isActive)
        {
            var p = await _context.Pickers.FindAsync(id);
            if (p == null) return NotFound();
            p.IsActive = isActive;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeletePicker(Guid id)
        {
            var p = await _context.Pickers.FindAsync(id);
            if (p == null) return NotFound();
            _context.Pickers.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========== ORCHARDS ===========
        [HttpGet("orchards")]
        public async Task<ActionResult<IEnumerable<Orchard>>> GetOrchards() => await _context.Orchards.ToListAsync();

        [HttpGet("orchards/active")]
        public async Task<ActionResult<IEnumerable<Orchard>>> GetActiveOrchards() =>
            await _context.Orchards.Where(o => o.IsActive).OrderBy(o => o.Name).ToListAsync();

        [HttpPost("orchards")]
        public async Task<ActionResult<Orchard>> CreateOrchard(Orchard orchard)
        {
            orchard.Id = Guid.NewGuid();
            orchard.IsActive = true;
            _context.Orchards.Add(orchard);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrchards), new { id = orchard.Id }, orchard);
        }

        [HttpPut("orchards/{id}")]
        public async Task<IActionResult> UpdateOrchard(Guid id, Orchard updated)
        {
            var o = await _context.Orchards.FindAsync(id);
            if (o == null) return NotFound();
            o.Name = updated.Name;
            o.Location = updated.Location;
            o.TotalArea = updated.TotalArea;
            o.IsActive = updated.IsActive;
            await _context.SaveChangesAsync();
            return Ok(o);
        }

        [HttpDelete("orchards/{id}")]
        public async Task<IActionResult> DeleteOrchard(Guid id)
        {
            var o = await _context.Orchards.FindAsync(id);
            if (o == null) return NotFound();
            _context.Orchards.Remove(o);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========== ORCHARD BLOCKS ===========
        [HttpGet("orchardblocks")]
        public async Task<ActionResult<IEnumerable<OrchardBlock>>> GetOrchardBlocks() => await _context.OrchardBlocks.ToListAsync();

        [HttpGet("orchardblocks/active")]
        public async Task<ActionResult<IEnumerable<OrchardBlock>>> GetActiveOrchardBlocks() =>
            await _context.OrchardBlocks.Where(ob => ob.IsActive).OrderBy(ob => ob.BlockName).ThenBy(ob => ob.BlockNumber).ToListAsync();

        [HttpPost("orchardblocks")]
        public async Task<ActionResult<OrchardBlock>> CreateOrchardBlock(OrchardBlock block)
        {
            block.Id = Guid.NewGuid();
            block.IsActive = true;
            _context.OrchardBlocks.Add(block);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrchardBlocks), new { id = block.Id }, block);
        }

        [HttpPut("orchardblocks/{id}")]
        public async Task<IActionResult> UpdateOrchardBlock(Guid id, OrchardBlock updated)
        {
            var b = await _context.OrchardBlocks.FindAsync(id);
            if (b == null) return NotFound();
            b.OrchardId = updated.OrchardId;
            b.BlockName = updated.BlockName;
            b.BlockNumber = updated.BlockNumber;
            b.AppleVariety = updated.AppleVariety;
            b.DefaultBinRate = updated.DefaultBinRate;
            b.IsActive = updated.IsActive;
            await _context.SaveChangesAsync();
            return Ok(b);
        }

        [HttpDelete("orchardblocks/{id}")]
        public async Task<IActionResult> DeleteOrchardBlock(Guid id)
        {
            var b = await _context.OrchardBlocks.FindAsync(id);
            if (b == null) return NotFound();
            _context.OrchardBlocks.Remove(b);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========== APPLE PRICES ===========
        [HttpGet("appleprices")]
        public async Task<ActionResult<IEnumerable<ApplePrice>>> GetApplePrices() => await _context.ApplePrices.ToListAsync();

        [HttpGet("appleprices/active")]
        public async Task<ActionResult<IEnumerable<ApplePrice>>> GetActiveApplePrices() =>
            await _context.ApplePrices.Where(ap => ap.IsActive).OrderBy(ap => ap.Variety).ToListAsync();

        [HttpGet("applevarieties")]
        public async Task<ActionResult<IEnumerable<string>>> GetAppleVarieties() =>
            await _context.ApplePrices.Where(ap => ap.IsActive).Select(ap => ap.Variety).Distinct().OrderBy(v => v).ToListAsync();

        [HttpGet("binrate/{variety}")]
        public async Task<ActionResult<decimal>> GetBinRate(string variety)
        {
            var price = await _context.ApplePrices.Where(ap => ap.Variety == variety && ap.IsActive).OrderByDescending(ap => ap.EffectiveDate).FirstOrDefaultAsync();
            return Ok(price?.BinRate ?? 45.00m);
        }

        [HttpPost("appleprices")]
        public async Task<ActionResult<ApplePrice>> CreateApplePrice(ApplePrice price)
        {
            price.Id = Guid.NewGuid();
            price.IsActive = true;
            _context.ApplePrices.Add(price);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetApplePrices), new { id = price.Id }, price);
        }

        [HttpPut("appleprices/{id}")]
        public async Task<IActionResult> UpdateApplePrice(Guid id, ApplePrice updated)
        {
            var p = await _context.ApplePrices.FindAsync(id);
            if (p == null) return NotFound();
            p.Variety = updated.Variety;
            p.PricePerKg = updated.PricePerKg;
            p.BinRate = updated.BinRate;
            p.EffectiveDate = updated.EffectiveDate;
            p.IsActive = updated.IsActive;
            await _context.SaveChangesAsync();
            return Ok(p);
        }

        [HttpDelete("appleprices/{id}")]
        public async Task<IActionResult> DeleteApplePrice(Guid id)
        {
            var p = await _context.ApplePrices.FindAsync(id);
            if (p == null) return NotFound();
            _context.ApplePrices.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========== PICK RECORDS ===========
        [HttpGet("pickrecords")]
        public async Task<ActionResult<IEnumerable<PickRecord>>> GetPickRecords() =>
            await _context.PickRecords.OrderByDescending(pr => pr.PickDate).ToListAsync();

        [HttpGet("pickrecords/admin")]
        public async Task<ActionResult<IEnumerable<PickRecord>>> GetAdminPickRecords() =>
            await _context.PickRecords.OrderByDescending(pr => pr.PickDate).ToListAsync();

        [HttpPost("pickrecords")]
        public async Task<ActionResult<PickRecord>> CreatePickRecord(PickRecord record)
        {
            record.Id = Guid.NewGuid();
            if (record.PickDate.Kind != DateTimeKind.Utc)
                record.PickDate = DateTime.SpecifyKind(record.PickDate, DateTimeKind.Utc);
            _context.PickRecords.Add(record);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPickRecords), new { id = record.Id }, record);
        }

        // =========== PACKHOUSES ===========
        [HttpGet("packhouses")]
        public async Task<ActionResult<IEnumerable<Packhouse>>> GetPackhouses() => await _context.Packhouses.ToListAsync();

        [HttpGet("packhouses/active")]
        public async Task<ActionResult<IEnumerable<Packhouse>>> GetActivePackhouses() =>
            await _context.Packhouses.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();

        [HttpPost("packhouses")]
        public async Task<ActionResult<Packhouse>> CreatePackhouse(Packhouse packhouse)
        {
            packhouse.Id = Guid.NewGuid();
            packhouse.IsActive = true;
            _context.Packhouses.Add(packhouse);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPackhouses), new { id = packhouse.Id }, packhouse);
        }

        [HttpPut("packhouses/{id}")]
        public async Task<IActionResult> UpdatePackhouse(Guid id, Packhouse updated)
        {
            var p = await _context.Packhouses.FindAsync(id);
            if (p == null) return NotFound();
            p.Name = updated.Name;
            p.Location = updated.Location;
            p.ContactPerson = updated.ContactPerson;
            p.Phone = updated.Phone;
            p.IsActive = updated.IsActive;
            await _context.SaveChangesAsync();
            return Ok(p);
        }

        [HttpDelete("packhouses/{id}")]
        public async Task<IActionResult> DeletePackhouse(Guid id)
        {
            var p = await _context.Packhouses.FindAsync(id);
            if (p == null) return NotFound();
            _context.Packhouses.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========== SEED DATABASE ===========
        [HttpPost("seed")]
        public async Task<IActionResult> SeedDatabase()
        {
            _context.Pickers.RemoveRange(_context.Pickers);
            _context.Orchards.RemoveRange(_context.Orchards);
            _context.OrchardBlocks.RemoveRange(_context.OrchardBlocks);
            _context.ApplePrices.RemoveRange(_context.ApplePrices);
            _context.PickRecords.RemoveRange(_context.PickRecords);
            _context.Packhouses.RemoveRange(_context.Packhouses);
            await _context.SaveChangesAsync();

            var pickers = new List<Picker>
            {
                new Picker { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "555-0101", IsActive = true, HireDate = DateTime.Today.AddMonths(-6) },
                new Picker { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Phone = "555-0102", IsActive = true, HireDate = DateTime.Today.AddMonths(-3) },
                new Picker { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Johnson", Email = "bob@example.com", Phone = "555-0103", IsActive = true, HireDate = DateTime.Today.AddMonths(-1) }
            };
            _context.Pickers.AddRange(pickers);

            var orchards = new List<Orchard>
            {
                new Orchard { Id = Guid.NewGuid(), Name = "Sunrise Orchard", Location = "North Valley", TotalArea = 50.5m, IsActive = true },
                new Orchard { Id = Guid.NewGuid(), Name = "Valley View", Location = "South Hills", TotalArea = 75.2m, IsActive = true },
                new Orchard { Id = Guid.NewGuid(), Name = "Golden Apple", Location = "West Ridge", TotalArea = 40.8m, IsActive = true }
            };
            _context.Orchards.AddRange(orchards);

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