using Microsoft.AspNetCore.Mvc;

namespace TheRocksNew.API.Controllers
{
    [ApiController]
    public class PickerController : ControllerBase
    {
        [HttpGet("api/picker")]
        public IActionResult Get()
        {
            return Ok("Picker API is working!");
        }
    }
}