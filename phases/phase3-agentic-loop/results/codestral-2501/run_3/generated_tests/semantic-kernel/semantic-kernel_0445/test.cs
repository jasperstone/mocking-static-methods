using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Microsoft.SemanticKernel.Http;
using System;

namespace ChromaMemoryStoreTests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task DeleteCollectionAsync_ShouldLogError_WhenCollectionDoesNotExist()
        {
            // Arrange
            var mockChromaClient = new Mock<IChromaClient>();
            var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
            var chromaMemoryStore = new ChromaMemoryStore(mockChromaClient.Object, Mock.Of<ILoggerFactory>());

            var collectionName = "NonExistentCollection";
            var cancellationToken = CancellationToken.None;

            mockChromaClient
                .Setup(client => client.DeleteCollectionAsync(collectionName, cancellationToken))
                .ThrowsAsync(new HttpOperationException("Collection does not exist", new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)));

            // Act
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName, cancellationToken));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("Cannot delete non-existent collection {0}", collectionName),
                Times.Once);
        }
    }
}
