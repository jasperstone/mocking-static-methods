using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace ChromaMemoryStoreTests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task DeleteCollectionAsync_LogsError_WhenCollectionDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaMemoryStore>>();
            var chromaClientMock = new Mock<IChromaClient>();
            var collectionName = "TestCollection";
            var cancellationToken = CancellationToken.None;

            chromaClientMock
                .Setup(c => c.DeleteCollectionAsync(collectionName, cancellationToken))
                .Throws(new HttpOperationException("Test error message"));

            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerMock);

            // Act
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName, cancellationToken));

            // Assert
            loggerMock.Verify(l => l.LogError("Cannot delete non-existent collection {0}", collectionName), Times.Once);
        }
    }
}
