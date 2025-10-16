using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PickerController : ControllerBase
{
    private readonly PickeAPIContext _context;

    public PickerController(PickeAPIContext context)
    {
        _context = context;
    }

    // GET: api/picker
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Picker>>> GetPickers()
    {
        return await _context.Pickers.ToListAsync();
    }

    // GET: api/picker/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Picker>> GetPicker(int id)
    {
        var picker = await _context.Pickers.FindAsync(id);

        if (picker == null)
        {
            return NotFound();
        }

        return picker;
    }

    // POST: api/picker
    [HttpPost]
    public async Task<ActionResult<Picker>> PostPicker(Picker picker)
    {
        _context.Pickers.Add(picker);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPicker), new { id = picker.Id }, picker);
    }

    // PUT: api/picker/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPicker(int id, Picker picker)
    {
        if (id != picker.Id)
        {
            return BadRequest();
        }

        _context.Entry(picker).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PickerExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/picker/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePicker(int id)
    {
        var picker = await _context.Pickers.FindAsync(id);
        if (picker == null)
        {
            return NotFound();
        }

        _context.Pickers.Remove(picker);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PickerExists(int id)
    {
        return _context.Pickers.Any(e => e.Id == id);
    }
}