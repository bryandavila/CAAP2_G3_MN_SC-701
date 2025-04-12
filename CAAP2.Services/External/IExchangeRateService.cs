using System.Threading.Tasks;
using CAAP2.Models.External;

namespace CAAP2.Services.External
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateResponse?> GetExchangeRateAsync();
    }
}

