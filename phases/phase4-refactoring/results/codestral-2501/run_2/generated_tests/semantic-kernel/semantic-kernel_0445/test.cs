using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Microsoft.SemanticKernel.Memory;
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
            var chromaMemoryStore = new ChromaMemoryStore(mockChromaClient.Object, new LoggerFactory());

            string collectionName = "NonExistentCollection";
            var cancellationToken = new CancellationToken();

            mockChromaClient
                .Setup(client => client.DeleteCollectionAsync(collectionName, cancellationToken))
                .ThrowsAsync(new HttpOperationException("Collection does not exist"));

            // Act
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName, cancellationToken));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("Cannot delete non-existent collection {0}", collectionName),
                Times.Once);
        }
    }
}
