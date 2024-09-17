using Microsoft.AspNetCore.Mvc;

namespace ShopAPI.Controllers
{
    [ApiController]
    public class ShopController : ControllerBase
    {

        [HttpPost]
        [Route("/trips/reserve")]
        public IActionResult Reserve([FromBody] UInt64 tripId)
        {

            return Ok("reserve");
        }

        [HttpPost]
        [Route("/trips/buy")]
        public IActionResult Buy([FromBody] UInt64 tripId)
        {
            return Ok("buy");
        }
    }
}
