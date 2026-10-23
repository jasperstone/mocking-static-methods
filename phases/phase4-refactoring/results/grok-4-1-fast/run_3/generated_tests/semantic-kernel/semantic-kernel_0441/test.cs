using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

public class ChromaClientTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly ChromaClient _client;

    public ChromaClientTests()
    {
        _mockLogger = new Mock<ILogger>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
        _client = new ChromaClient("http://localhost:8000", mockLoggerFactory.Object);
    }

    [Fact]
    public async Task GetEmbeddingsAsync_LogsDebugMessage()
    {
        // Arrange
        string collectionId = "test-collection";
        string[] ids = { "id1" };

        // Act
        await _client.GetEmbeddingsAsync(collectionId, ids);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v != null && v.ToString()!.Contains($"Getting embeddings from collection with id: {collectionId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
