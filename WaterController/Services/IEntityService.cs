using System.Collections.Generic;
using System.Threading.Tasks;
using ContextBrokerLibrary.Model;

namespace WaterController.Services
{
    public interface IEntityService
    {
        Task<RetrieveBedEntityResponse> GetFlowerBed(string bedId);
        
        Task<List<ListEntitiesResponse>> GetMoistureLevels(string bedId);

        Task SendCommand(string valveId, string command);
    }
}