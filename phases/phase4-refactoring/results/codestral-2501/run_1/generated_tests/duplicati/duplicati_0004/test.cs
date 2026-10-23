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
        var mockHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHandler.Object);
        var jsonWebHelper = new JsonWebHelperHttpClient(httpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        mockHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>(
                       "SendAsync",
                       ItExpr.IsAny<HttpRequestMessage>(),
                       ItExpr.IsAny<CancellationToken>()
                   )
                   .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK))
                   .Verifiable();

        // Act
        var response = await jsonWebHelper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
