using System;
using System.Collections.Generic;
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
            var mockChromaClient = new Mock<IChromaClient>();
            var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
            var collectionName = "NonExistentCollection";

            mockChromaClient
                .Setup(client => client.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpOperationException
                {
                    ResponseContent = "Collection does not exist"
                });

            var chromaMemoryStore = new ChromaMemoryStore(mockChromaClient.Object, new LoggerFactory().AddProvider(new MockLoggerProvider(mockLogger.Object)));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName));

            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Cannot delete non-existent collection {0}")),
                    collectionName),
                Times.Once);
        }
    }

    // Mock logger provider to integrate with Moq
    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly Mock<ILogger> _loggerMock;

        public MockLoggerProvider(Mock<ILogger> loggerMock)
        {
            _loggerMock = loggerMock;
        }

        public ILogger CreateLogger(string categoryName) => _loggerMock.Object;

        public void Dispose() { }
    }
}
