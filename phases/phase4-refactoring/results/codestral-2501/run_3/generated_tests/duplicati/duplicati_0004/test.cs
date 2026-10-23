using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Xunit;

public class JsonWebHelperHttpClientTests
{
    [Fact]
    public async Task GetResponseUncheckedAsync_ShouldCallSendAsync()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage());

        var jsonWebHelper = new JsonWebHelperHttpClient(httpClient);

        // Act
        await jsonWebHelper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
