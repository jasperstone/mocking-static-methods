using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ChromaMemoryStoreTests
{
    [Fact]
    public async Task DeleteCollectionAsync_LogsError_WhenCollectionDoesNotExist()
    {
        // Arrange
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockChromaClient = new Mock<IChromaClient>();
        var exception = new HttpOperationException
        {
            ResponseContent = "Collection does not exist"
        };
        mockChromaClient
            .Setup(c => c.DeleteCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var chromaMemoryStore = new ChromaMemoryStore(mockChromaClient.Object, mockLoggerFactory.Object);

        // Act
        await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync("NonExistentCollection"));

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.Is<string>(s => s.Contains("Cannot delete non-existent collection NonExistentCollection")),
                "NonExistentCollection"),
            Times.Once);
    }
}
