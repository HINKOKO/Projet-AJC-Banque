using System.Configuration;
using System.Text.Json;

namespace ServeurAJCBanque.Services
{
    public class ExchangeRate
    {
        private readonly string apiUrl;
        private readonly string apiKey;

        public ExchangeRate()
        {
            apiUrl = ConfigurationManager.AppSettings["ExchangeRateUrl"];
            apiKey = ConfigurationManager.AppSettings["ExchangeRateApiKey"];
        }
        public async Task<decimal> GetExchangeRateAsync(string devise)
        {
            using HttpClient client = new HttpClient();
            string requestUrl = $"{apiUrl}{apiKey}/latest/{devise}";

            try
            {
                var response = await client.GetAsync(requestUrl);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);
                if (jsonData.TryGetValue("conversion_rates", out var rates) && rates is JsonElement rateElement)
                {
                    if (rateElement.TryGetProperty("EUR", out var rateValue))
                    {
                        return rateValue.GetDecimal();
                    }
                    else
                    {
                        throw new Exception($"Taux de change pour {devise} non trouvé");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur API taux de change -> " + ex.Message);
                throw;
            }
            return 1;
        }
    }
}
