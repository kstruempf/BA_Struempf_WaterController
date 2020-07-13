using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace WaterController.Services.Impl
{
    public class ValveService : IValveService
    {
        // TODO make ids configurable
        private const string ValveId = "urn:ngsi-ld:Valve:001";
        private const string BedId = "urn:ngsi-ld:Bed:001";
        private const double MinimumRainIn1H = 0.2;
        private const double MinimumRainIn2H = 0.5;
        private const double MinimumRainIn1D = 1.5;
        private const double MinimumRainIn2D = 5;

        private readonly ILogger<ValveService> _logger;
        private readonly IEntityService _entityService;


        public ValveService(ILogger<ValveService> logger, IEntityService entityService)
        {
            _logger = logger;
            _entityService = entityService;
        }

        public async Task<string> OpenValveIfRequired()
        {
            _logger.LogInformation("Checking if vale should be opened...");

            var levels = (await _entityService.GetMoistureLevels(BedId))
                .Select(l => l.SufficientSufficientMoisture?.Value)
                .ToList();

            _logger.LogDebug("Got [{@levels}] moisture levels", levels);

            string msg;
            if (!levels.Contains("not_sufficient"))
            {
                msg = $"Flower bed {BedId} is sufficiently watered";
                _logger.LogInformation(msg);
                return msg;
            }

            var flowerBed = await _entityService.GetFlowerBed(BedId);

            if (flowerBed.ExpRainVolume1H.Value > MinimumRainIn1H)
            {
                msg = $"Expecting {flowerBed.ExpRainVolume1H.Value} mm rain in 1 hour - not watering";
                _logger.LogInformation(msg);
                return msg;
            }

            if (flowerBed.ExpRainVolume2H.Value > MinimumRainIn2H)
            {
                msg = $"Expecting {flowerBed.ExpRainVolume2H.Value} mm rain in 2 hours - not watering";
                _logger.LogInformation(msg);
                return msg;
            }

            if (flowerBed.ExpRainVolume1D.Value < MinimumRainIn1D)
            {
                msg = $"Expecting {flowerBed.ExpRainVolume1D.Value} mm rain in 1 day - watering for 15 min";
                _logger.LogInformation(msg);

                await _entityService.SendCommand(ValveId, "open");

                BackgroundJob.Schedule<IEntityService>(s => s.SendCommand(ValveId, "close"), TimeSpan.FromMinutes(15));
                return msg;
            }

            if (flowerBed.ExpRainVolume2D.Value < MinimumRainIn2D)
            {
                msg = $"Expecting {flowerBed.ExpRainVolume2D.Value} mm rain in 2 days - watering for 25 min";
                _logger.LogInformation(msg);

                await _entityService.SendCommand(ValveId, "open");

                BackgroundJob.Schedule<IEntityService>(s => s.SendCommand(ValveId, "close"), TimeSpan.FromMinutes(25));
            }

            msg = "No action taken.";
            _logger.LogInformation(msg);
            return msg;
        }
    }
}