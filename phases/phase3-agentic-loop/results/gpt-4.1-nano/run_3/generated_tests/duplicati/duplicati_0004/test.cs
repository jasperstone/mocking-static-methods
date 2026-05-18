using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Duplicati.Library;

public class JsonWebHelperHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly JsonWebHelperHttpClient _helper;

    public JsonWebHelperHttpClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _helper = new JsonWebHelperHttpClient(_httpClient);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_CallsSendAsync_ReturnsResponse()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
        var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        _httpMessageHandlerMock
            .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
            .Returns<HttpRequestMessage>(req => responseMessage);

        // Act
        var result = await _helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

        // Assert
        Assert.Equal(responseMessage, result);
        _httpMessageHandlerMock.Verify(m => m.Send(It.Is<HttpRequestMessage>(r => r == request)), Times.Once);
    }
}
