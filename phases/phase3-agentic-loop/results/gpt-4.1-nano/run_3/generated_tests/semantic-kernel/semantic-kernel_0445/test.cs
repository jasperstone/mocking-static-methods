using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace ChromaMemoryStoreTests
{
    public class ChromaMemoryStoreTests
    {
        private readonly Mock<IChromaClient> _mockClient;
        private readonly Mock<ILogger<ChromaMemoryStore>> _mockLogger;
        private readonly ChromaMemoryStore _store;

        public ChromaMemoryStoreTests()
        {
            _mockClient = new Mock<IChromaClient>();
            _mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
            _store = new ChromaMemoryStore(_mockClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task DeleteCollectionAsync_WhenHttpOperationExceptionAndVerifyCollectionDoesNotExistMessage_ShouldLogErrorAndThrow()
        {
            // Arrange
            var collectionName = "testCollection";
            var exceptionResponseContent = "Collection does not exist";
            var exception = new HttpOperationException { ResponseContent = exceptionResponseContent };
            _mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);
            _mockClient.Setup(c => c.GetCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryCollection { Id = "id" });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KernelException>(() => _store.DeleteCollectionAsync(collectionName));
            Assert.Contains(collectionName, ex.Message);
            _mockLogger.Verify(
                x => x.LogError("Cannot delete non-existent collection {0}", collectionName),
                Times.Once);
        }
    }
}
