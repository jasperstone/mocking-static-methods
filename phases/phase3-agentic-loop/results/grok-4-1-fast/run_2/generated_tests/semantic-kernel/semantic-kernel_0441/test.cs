using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.UnitTests;

public class ChromaClientTests
{
    private readonly Mock<ILogger<ChromaClient>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly ChromaClient _client;

    public ChromaClientTests()
    {
        _loggerMock = new Mock<ILogger<ChromaClient>>();
        _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        _handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_handlerMock.Object);
        httpClient.BaseAddress = new Uri("http://test-endpoint/");
        
        _client = new ChromaClient(httpClient, loggerFactory: new TestLoggerFactory(_loggerMock.Object));
    }

    [Fact]
    public async Task GetEmbeddingsAsync_LogsDebugMessageWithCollectionId()
    {
        // Arrange
        var collectionId = "test-collection";
        var ids = new[] { "id1" };

        // Setup HTTP handler to respond successfully
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ids\":[],\"embeddings\":[],\"metadatas\":[],\"documents\":[]}")
            });

        // Act
        await _client.GetEmbeddingsAsync(collectionId, ids);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Getting embeddings from collection with id: {collectionId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetEmbeddingsAsync_DoesNotLog_WhenDebugNotEnabled()
    {
        // Arrange
        var collectionId = "test-collection";
        var ids = new[] { "id1" };
        _loggerMock.Reset();
        _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

        // Setup HTTP handler to respond successfully
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ids\":[],\"embeddings\":[],\"metadatas\":[],\"documents\":[]}")
            });

        // Act
        await _client.GetEmbeddingsAsync(collectionId, ids);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}

internal class TestLoggerFactory : ILoggerFactory
{
    private readonly ILogger _logger;

    public TestLoggerFactory(ILogger logger)
    {
        _logger = logger;
    }

    public void Dispose() { }

    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => _logger;
}
