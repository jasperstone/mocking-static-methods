using System.Net.Http;
using System.Threading.Tasks;

namespace Acme;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> FetchAsync(string url)
    {
        HttpResponseMessage resp = await _http.GetAsync(url);
        return await resp.Content.ReadAsStringAsync();
    }
}
