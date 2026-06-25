using System.Net.Http;
using System.Threading.Tasks;

namespace Acme;

/// <summary>
/// Test case for static utility wrapper pattern.
/// 
/// This demonstrates the ENHANCEMENT: wrapping an external framework type
/// (HttpClient) that has no source and cannot be injected directly.
/// 
/// BEFORE: This call could not be refactored (no_receiver_source rejection)
/// AFTER:  Creates IHttpClientWrapper and injects it, allowing mocking
/// </summary>
public class ApiClient
{
    private readonly HttpClient _client;

    public ApiClient()
    {
        // HttpClient created internally - not injectable parameter
        _client = new HttpClient();
    }

    public async Task<string> FetchOrderAsync(string orderId)
    {
        // This static utility call (HttpClient.GetAsync) previously rejected:
        // - Receiver (_client) is HttpClient (no source, framework type)
        // - Not a constructor parameter or injectable field
        // - Would have been rejected with "no_receiver_source"
        //
        // With enhancement:
        // - Tool recognizes this is framework type
        // - Creates IHttpClientWrapper interface
        // - Wraps HttpClient and injects wrapper
        // - Rewrites call to use wrapper
        HttpResponseMessage resp = await _client.GetAsync($"https://api.example.com/orders/{orderId}");
        return await resp.Content.ReadAsStringAsync();
    }

    public async Task<string> PostOrderAsync(string orderId, string payload)
    {
        // Another call on same receiver - should be rewritten too
        var content = new StringContent(payload);
        HttpResponseMessage resp = await _client.PostAsync($"https://api.example.com/orders/{orderId}", content);
        return await resp.Content.ReadAsStringAsync();
    }
}
