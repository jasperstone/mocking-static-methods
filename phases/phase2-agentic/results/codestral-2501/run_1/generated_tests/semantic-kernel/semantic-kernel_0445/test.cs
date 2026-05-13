using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Memory;
using Moq;
using Xunit;

namespace ChromaMemoryStoreTests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task DeleteCollectionAsync_ShouldLogError_WhenCollectionDoesNotExist()
        {
            // Arrange
            var collectionName = "NonExistentCollection";
            var mockChromaClient = new Mock<IChromaClient>();
            var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
            var chromaMemoryStore = new ChromaMemoryStore(mockChromaClient.Object, NullLoggerFactory.Instance);

            mockChromaClient.Setup(client => client.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpOperationException("Collection does not exist", new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)));

            // Act
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("Cannot delete non-existent collection {0}", collectionName),
                Times.Once);
        }
    }
}
