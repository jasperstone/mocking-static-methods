using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
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
                .Throws(new HttpOperationException("Collection not found"));

            var loggerFactory = new LoggerFactory();
            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactory.CreateLogger<ChromaMemoryStore>());

            // Act
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName, cancellationToken));

            // Assert
            loggerMock.Verify(l => l.LogError("Cannot delete non-existent collection {0}", collectionName), Times.Once);
        }
    }
}
