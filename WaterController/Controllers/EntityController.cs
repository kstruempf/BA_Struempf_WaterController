using System.Threading.Tasks;
using ContextBrokerLibrary.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WaterController.Services;

namespace WaterController.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntityController : ControllerBase
    {
        private readonly ILogger<EntityController> _logger;
        private readonly IEntityService _entityService;

        public EntityController(ILogger<EntityController> logger, IEntityService entityService)
        {
            _logger = logger;
            _entityService = entityService;
        }

        [HttpGet("{id}")]
        public async Task<object> Get(string id)
        {
            _logger.LogInformation("Loading entity {entityId}", id);

            try
            {
                return await _entityService.GetFlowerBed(id);
            }
            catch (ApiException e)
            {
                _logger.LogWarning("({errorCode}) Failed to get entity with id {entityId} - {errorMessage}",
                    e.ErrorCode, id, e.Message);

                return StatusCode(e.ErrorCode);
            }
        }

        [HttpGet("sensorReadings/{id}")]
        public async Task<object> GetSensorReadings(string id)
        {
            _logger.LogInformation("Getting latest sensor readings for {entityId}", id);

            try
            {
                return await _entityService.GetMoistureLevels(id);
            }
            catch (ApiException e)
            {
                _logger.LogWarning("({errorCode}) Failed to get readings for {entityId} - {errorMessage}",
                    e.ErrorCode, id, e.Message);

                return StatusCode(e.ErrorCode);
            }
        }

        [HttpGet("command/{command}/{id}")]
        public async Task<object> OpenValve(string id, string command)
        {
            try
            {
                await _entityService.SendCommand(id, command);
                return Ok(command);
            }
            catch (ApiException e)
            {
                _logger.LogWarning("({errorCode}) Failed to get readings for {entityId} - {errorMessage}",
                    e.ErrorCode, id, e.Message);

                return StatusCode(e.ErrorCode);
            }
        }
    }
}