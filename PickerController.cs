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

        // GET: api/picker - Health check
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Picker API is working!");
        }

        // GET: api/picker/admin/all - Get all pickers (admin view)
        [HttpGet("admin/all")]
        public async Task<ActionResult<IEnumerable<Picker>>> GetAllPickers()
        {
            return await _context.Pickers.ToListAsync();
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
    }
}