// Test case: HttpClient static utility wrapping
using System.Net.Http;
using System.Threading.Tasks;

namespace TestProject
{
    public class OrderService
    {
        private readonly HttpClient _client;

        public OrderService()
        {
            _client = new HttpClient();
        }

        public async Task<string> FetchOrder(string id)
        {
            // This call currently can't be mocked because:
            // - HttpClient is framework type (no source)
            // - Receiver has no constructor-injectable source
            // With our enhancement, this should now be wrappable via static utility pattern
            var response = await _client.GetAsync($"https://api.example.com/orders/{id}");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
