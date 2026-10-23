using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.Chroma;

public class ChromaMemoryStoreTests
{
    [Fact]
    public async Task DeleteCollectionAsync_ShouldLogError_WhenHttpOperationExceptionIndicatesCollectionDoesNotExist()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        var store = new ChromaMemoryStore(mockClient.Object, null);
        var collectionName = "testCollection";

        var exception = new HttpOperationException("Error", HttpStatusCode.NotFound, "Collection does not exist");
        mockClient.Setup(c => c.DeleteCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(exception);

        // Act
        await Assert.ThrowsAsync<KernelException>(() => store.DeleteCollectionAsync(collectionName));

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cannot delete non-existent collection {collectionName}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
