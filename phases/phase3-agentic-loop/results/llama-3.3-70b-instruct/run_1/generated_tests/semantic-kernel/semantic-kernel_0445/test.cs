using Microsoft.Extensions.Logging;
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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ChromaMemoryStore>>();
            loggerFactoryMock
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);
            var chromaClientMock = new Mock<IChromaClient>();
            var collectionName = "test-collection";
            var cancellationToken = CancellationToken.None;

            chromaClientMock
                .Setup(c => c.DeleteCollectionAsync(collectionName, cancellationToken))
                .Throws(new HttpOperationException("Collection not found"));

            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactoryMock.Object);

            // Act and Assert
            try
            {
                await chromaMemoryStore.DeleteCollectionAsync(collectionName, cancellationToken);
            }
            catch (HttpOperationException)
            {
                loggerMock.Verify(l => l.LogError("Cannot delete non-existent collection {0}", collectionName), Times.Once);
            }
        }
    }
}
