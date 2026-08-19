using Xunit;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Duplicati.Library;

public class JsonWebHelperHttpClientTests
{
    [Fact]
    public async Task GetResponseUncheckedAsync_ShouldCallSendAsync()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        // Act
        await jsonWebHelperHttpClient.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // Assert
        mockHttpMessageHandler.Verify(
            handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
