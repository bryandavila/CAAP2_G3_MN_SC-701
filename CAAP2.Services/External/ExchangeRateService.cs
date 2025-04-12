using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CAAP2.Models.External;

namespace CAAP2.Services.External
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;

        public ExchangeRateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ExchangeRateResponse?> GetExchangeRateAsync()
        {
            var url = "https://tipodecambio.paginasweb.cr/api/";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ExchangeRateResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }

    }
}

