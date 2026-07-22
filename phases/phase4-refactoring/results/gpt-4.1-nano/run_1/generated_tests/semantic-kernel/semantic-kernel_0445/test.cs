using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Chroma;

public class ChromaMemoryStoreTests
{
    [Fact]
    public async Task DeleteCollectionAsync_ShouldLogError_WhenHttpOperationExceptionIndicatesNonExistentCollection()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        var loggerFactory = new LoggerFactory();
        var store = new ChromaMemoryStore(mockClient.Object, loggerFactory);

        var collectionName = "testCollection";

        var exceptionResponseContent = "Collection does not exist";

        mockClient
            .Setup(c => c.DeleteCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpOperationException(exceptionResponseContent));

        // Act
        await Assert.ThrowsAsync<KernelException>(() => store.DeleteCollectionAsync(collectionName));

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cannot delete non-existent collection {collectionName}")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
