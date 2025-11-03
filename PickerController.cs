using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheRocksNew.API.Data;
using TheRocksNew.API.Models;

namespace TheRocksNew.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PickerController : ControllerBase
    {
        private readonly PickerAPIContext _context;

        public PickerController(PickerAPIContext context)
        {
            _context = context;
        }

        // GET: api/Picker
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Picker>>> GetPicker()
        {
            // Fix: Return ActionResult with the list
            return await _context.Pickers.ToListAsync();
        }

        // GET: api/Picker/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Picker>> GetPicker(int id)
        {
            var picker = await _context.Pickers.FindAsync(id);

            if (picker == null)
            {
                return NotFound();
            }

            // Fix: Return ActionResult with the object
            return picker;
        }

        // POST: api/Picker
        [HttpPost]
        public async Task<ActionResult<Picker>> PostPicker(Picker picker)
        {
            // Fix: Make sure you're using the correct type
            // If there's a mismatch between Data.Picker and Models.Picker,
            // you may need to map between them
            _context.Pickers.Add(picker);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPicker", new { id = picker.Id }, picker);
        }
    }
}