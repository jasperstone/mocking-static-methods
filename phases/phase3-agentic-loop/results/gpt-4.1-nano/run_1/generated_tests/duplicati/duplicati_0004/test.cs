using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Duplicati.Library;

public class JsonWebHelperHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly JsonWebHelperHttpClient _helper;

    public JsonWebHelperHttpClientTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_handlerMock.Object);
        _helper = new JsonWebHelperHttpClient(_httpClient);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_Should_Call_SendAsync_And_Return_Response()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
        var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        _handlerMock
            .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

        // Assert
        Assert.Equal(responseMessage, result);
        _handlerMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
