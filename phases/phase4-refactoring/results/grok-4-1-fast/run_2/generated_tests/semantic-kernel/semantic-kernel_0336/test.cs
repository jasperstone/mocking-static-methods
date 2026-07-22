using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Tests.Brave;

public class BraveConnectorTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger> _loggerMock;
    private readonly string _apiKey = "test-api-key";

    public BraveConnectorTests()
    {
        this._httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        this._httpClient = new HttpClient(this._httpMessageHandlerMock.Object);
        this._loggerMock = new Mock<ILogger>();
        this._loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
    }

    [Fact]
    public async Task SearchAsync_LogsTraceForResponseContent_WhenTraceEnabled()
    {
        // Arrange
        var jsonResponse = "{\"type\":\"search\",\"web\":{\"type\":\"search\",\"results\":[{\"title\":\"Test\",\"description\":\"Test desc\",\"url\":\"https://test.com\"}]}}";
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json") };

        this._httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(this._loggerMock.Object);
        var connector = new BraveConnector(this._apiKey, this._httpClient, null, loggerFactory.Object);

        // Act
        await connector.SearchAsync<string>("test query");

        // Assert
        this._loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Response content received:") && state.ToString()!.Contains(jsonResponse)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_LogsDebugMessages_WhenCalled()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };

        this._httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(this._loggerMock.Object);
        var connector = new BraveConnector(this._apiKey, this._httpClient, null, loggerFactory.Object);

        // Act
        await connector.SearchAsync<string>("test query");

        // Assert
        this._loggerMock.Verify(l => l.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountInvalid(int count)
    {
        // Arrange
        var connector = new BraveConnector(this._apiKey, this._httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => connector.SearchAsync<string>("test", count));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetInvalid(int offset)
    {
        // Arrange
        var connector = new BraveConnector(this._apiKey, this._httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => connector.SearchAsync<string>("test", offset: offset));
    }
}
