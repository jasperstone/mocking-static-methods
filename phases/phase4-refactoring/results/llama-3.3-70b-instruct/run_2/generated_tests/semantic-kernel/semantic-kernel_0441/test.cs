using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;

public class ChromaClientTests
{
    [Fact]
    public async Task GetEmbeddingsAsync_LogsDebugMessage()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var loggerMock = new Mock<ILogger<ChromaClient>>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        var httpClient = new HttpClient(handlerMock.Object);
        var chromaClient = new ChromaClient(httpClient, "https://example.com", loggerFactoryMock.Object);

        // Act
        await chromaClient.GetEmbeddingsAsync("collectionId", new[] { "id1" }, null, default);

        // Assert
        loggerMock.Verify(logger => logger.LogDebug("Getting embeddings from collection with id: {0}", "collectionId"), Times.Once);
    }
}
