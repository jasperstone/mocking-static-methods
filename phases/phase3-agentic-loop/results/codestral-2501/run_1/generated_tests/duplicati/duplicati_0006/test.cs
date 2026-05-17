using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Moq.Protected;
using Xunit;

public class OAuthHttpClientTests
{
    [Fact]
    public async Task SendAsync_ShouldThrowTimeoutException_WhenOperationCanceledExceptionIsThrown()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new OperationCanceledException());

        var client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");

        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, true, cancellationToken));
    }

    [Fact]
    public async Task SendAsync_ShouldReturnResponse_WhenRequestIsValid()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");

        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        // Act
        var result = await client.SendAsync(request, true, cancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
    }
}
