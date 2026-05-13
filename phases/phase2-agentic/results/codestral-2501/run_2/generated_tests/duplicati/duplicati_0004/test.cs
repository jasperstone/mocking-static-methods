using Xunit;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using System.Net;
using Duplicati.Library;

public class JsonWebHelperHttpClientTests
{
    [Fact]
    public async Task GetResponseUncheckedAsync_ShouldCallSendAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(mockHttpClient.Object);

        // Act
        var response = await jsonWebHelperHttpClient.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // Assert
        mockHttpClient.Verify(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
