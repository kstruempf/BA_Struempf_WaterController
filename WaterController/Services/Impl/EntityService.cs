using System.Collections.Generic;
using System.Threading.Tasks;
using ContextBrokerLibrary.Api;
using ContextBrokerLibrary.Model;
using Microsoft.Extensions.Logging;

namespace WaterController.Services.Impl
{
    public class EntityService : IEntityService
    {
        private readonly IEntitiesApi _entitiesApi;
        private readonly ILogger<EntityService> _logger;

        public EntityService(IEntitiesApi entitiesApi, ILogger<EntityService> logger)
        {
            _entitiesApi = entitiesApi;
            _logger = logger;
        }

        public async Task<RetrieveBedEntityResponse> GetFlowerBed(string bedId)
        {
            _logger.LogInformation("Getting flower bed {id}", bedId);

            return await _entitiesApi.RetrieveBedEntityAsync(bedId, "FlowerBed");
        }

        public async Task<List<ListEntitiesResponse>> GetMoistureLevels(string bedId)
        {
            _logger.LogInformation("Getting latest moisture levels from bed {id}", bedId);
            return await _entitiesApi.ListEntitiesAsync(null, "Sensor", null, null, $"refBed=={bedId}");
        }

        public async Task SendCommand(string valveId, string command)
        {
            _logger.LogInformation("Sending command {command} to valve {id}", command, valveId);

            UpdateExistingEntityAttributesRequest commandObj;

            if (command == "open")
            {
                commandObj = new OpenCommand();
            }
            else
            {
                commandObj = new CloseCommand();
            }

            var response = await _entitiesApi.UpdateExistingEntityAttributesAsyncWithHttpInfo(
                commandObj,
                "", valveId);

            _logger.LogDebug("Got response {statusCode}", response.StatusCode);
        }
    }
}