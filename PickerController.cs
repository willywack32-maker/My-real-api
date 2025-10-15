using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using System.Diagnostics;
using TheRocksNew.API.Data;

namespace TheRocksNew.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PickersController : ControllerBase
    {
        private readonly Data.PickerAPIContext dbContext;

        public PickersController(PickerAPIContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetPickers()
        {
            return Ok(await dbContext.Pickers.ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPicker([FromRoute] int id)
        {
            var picker = await dbContext.Pickers.FindAsync(id);
            return picker == null ? NotFound() : Ok(picker);
        }
        [HttpPost]
        public async Task<IActionResult> AddPicker([FromBody] Picker picker)

        {
            Debug.WriteLine($"Received AddPicker: {System.Text.Json.JsonSerializer.Serialize(picker)} at {DateTime.Now}");
            if (picker == null || string.IsNullOrWhiteSpace(picker.Name) || string.IsNullOrWhiteSpace(picker.OrchardName) || string.IsNullOrWhiteSpace(picker.PackHouse))
            {
                return BadRequest("Missing required fields");
            }
            dbContext.Pickers.Add(picker);
            await dbContext.SaveChangesAsync();
            return Ok(picker);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePicker([FromRoute] int id, [FromBody] Picker updatedPicker)
        {
            if (updatedPicker == null)
                return BadRequest("Picker data is null");

            if (string.IsNullOrWhiteSpace(updatedPicker.Name) || string.IsNullOrWhiteSpace(updatedPicker.OrchardName) || string.IsNullOrWhiteSpace(updatedPicker.PackHouse))
                return BadRequest("Name, OrchardName, and PackHouse are required");

            var existingPicker = await dbContext.Pickers.FindAsync(id);
            if (existingPicker == null)
                return NotFound();

            existingPicker.Name = updatedPicker.Name ?? existingPicker.Name;
            existingPicker.AppleType = updatedPicker.AppleType;
            existingPicker.OrchardName = updatedPicker.OrchardName ?? existingPicker.OrchardName;
            existingPicker.HoursWorked = updatedPicker.HoursWorked;
            existingPicker.BinRate = updatedPicker.BinRate;
            existingPicker.PackHouse = updatedPicker.PackHouse ?? existingPicker.PackHouse;

            try
            {
                await dbContext.SaveChangesAsync();
                return Ok(existingPicker);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Picker was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdatePicker error: {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, "An error occurred while updating the picker");
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePicker([FromRoute] int id)
        {
            var picker = await dbContext.Pickers.FindAsync(id);
            if (picker == null) return NotFound();

            dbContext.Pickers.Remove(picker);
            await dbContext.SaveChangesAsync();
            return Ok(picker);
        }
        [HttpPost("TestPost")]
        public IActionResult TestPost()
        {
            return Ok("POST works");
        }
    }
}