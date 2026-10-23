using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Library.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseUncheckedAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_InvalidRequest_ThrowsException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);
            var request = new HttpRequestMessage(HttpMethod.Get, "invalid-url");

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => jsonWebHelperHttpClient.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None));
        }
    }
}
