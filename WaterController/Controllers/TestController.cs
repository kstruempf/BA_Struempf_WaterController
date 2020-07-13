using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WaterController.Services;

namespace WaterController.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IValveService _valveService;

        public TestController(IValveService valveService)
        {
            _valveService = valveService;
        }

        [HttpGet("ping")]
        public object Ping()
        {
            return Ok("pong");
        }

        [HttpGet("watering")]
        public async Task<IActionResult> CheckWateringRequired()
        {
            return Ok(await _valveService.OpenValveIfRequired());
        }
    }
}