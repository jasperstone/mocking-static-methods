using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.SemanticKernel.Diagnostics;

namespace ChromaMemoryStoreTests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task DeleteCollectionAsync_LogsError_WhenCollectionDoesNotExist()
        {
            // Arrange
            var mockChromaClient = new Mock<IChromaClient>();
            var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
            var chromaMemoryStore = new ChromaMemoryStore(mockChromaClient.Object, Mock.Of<ILoggerFactory>());

            string collectionName = "NonExistentCollection";
            mockChromaClient.Setup(client => client.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Collection does not exist"));

            // Act
            await Assert.ThrowsAsync<Exception>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("Cannot delete non-existent collection {0}", collectionName),
                Times.Once);
        }
    }
}
