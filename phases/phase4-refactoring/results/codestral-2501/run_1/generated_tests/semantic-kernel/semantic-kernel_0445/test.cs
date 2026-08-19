using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task DeleteCollectionAsync_LogsError_WhenCollectionDoesNotExist()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
            var mockChromaClient = new Mock<IChromaClient>();
            var collectionName = "NonExistentCollection";

            mockChromaClient
                .Setup(client => client.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpOperationException("Collection does not exist"));

            var chromaMemoryStore = new ChromaMemoryStore(mockChromaClient.Object, Mock.Of<ILoggerFactory>());

            // Act
            await Assert.ThrowsAsync<HttpOperationException>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("Cannot delete non-existent collection {0}", collectionName),
                Times.Once);
        }
    }
}
