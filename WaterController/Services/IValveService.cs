using System.Threading.Tasks;

namespace WaterController.Services
{
    public interface IValveService
    {
        public Task<string> OpenValveIfRequired();
    }
}