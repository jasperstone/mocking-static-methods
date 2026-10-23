using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
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
                .Setup(l => l.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);
            var chromaClientMock = new Mock<IChromaClient>();
            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactoryMock.Object);

            chromaClientMock
                .Setup(c => c.DeleteCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Collection does not exist"));

            // Act
            await Assert.ThrowsAsync<Exception>(() => chromaMemoryStore.DeleteCollectionAsync("non-existent-collection", CancellationToken.None));

            // Assert
            loggerMock
                .Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
