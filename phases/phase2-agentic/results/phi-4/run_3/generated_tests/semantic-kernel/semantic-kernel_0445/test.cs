using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(mockLogger.Object);

            var mockChromaClient = new Mock<IChromaClient>();
            mockChromaClient
                .Setup(c => c.DeleteCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpOperationException(new HttpResponseMessage(), "Collection does not exist"));

            var chromaMemoryStore = new ChromaMemoryStore(mockChromaClient.Object, mockLoggerFactory.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync("NonExistentCollection"));

            mockLogger.Verify(
                l => l.LogError(It.Is<string>(s => s.Contains("Cannot delete non-existent collection NonExistentCollection"))),
                Times.Once);
        }
    }
}
