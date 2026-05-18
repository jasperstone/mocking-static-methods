using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Duplicati.Library;

public class JsonWebHelperHttpClientTests
{
    [Fact]
    public async Task GetResponseUncheckedAsync_CallsSendAsyncAndReturnsResponse()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var expectedResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        mockHttpMessageHandler
            .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var helper = new JsonWebHelperHttpClient(httpClient);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockHttpMessageHandler.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
