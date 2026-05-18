using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System;
using Microsoft.SemanticKernel.Http;
using System.Net;
using Microsoft.SemanticKernel.Exceptions;

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
            var collectionName = "NonExistentCollection";

            mockChromaClient
                .Setup(client => client.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpOperationException("Collection does not exist", new HttpResponseMessage(HttpStatusCode.NotFound)));

            var chromaMemoryStore = new ChromaMemoryStore(mockChromaClient.Object, Mock.Of<ILoggerFactory>());

            // Act
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("Cannot delete non-existent collection {0}", collectionName),
                Times.Once);
        }
    }
}
