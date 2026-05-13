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
        private readonly Mock<ILogger> _mockLogger;
        private readonly ChromaMemoryStore _store;

        public ChromaMemoryStoreTests()
        {
            _mockClient = new Mock<IChromaClient>();
            _mockLogger = new Mock<ILogger>();
            _store = new ChromaMemoryStore(_mockClient.Object, new LoggerFactory().AddProvider(new MockLoggerProvider(_mockLogger.Object)));
        }

        [Fact]
        public async Task DeleteCollectionAsync_WhenHttpOperationExceptionAndVerifyMessage_ShouldLogErrorAndThrow()
        {
            // Arrange
            string collectionName = "testCollection";
            var exception = new HttpOperationException { ResponseContent = "Collection does not exist" };
            _mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);
            _mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .Callback(() => throw new HttpOperationException { ResponseContent = "Collection does not exist" });

            // Act & Assert
            await Assert.ThrowsAsync<KernelException>(() => _store.DeleteCollectionAsync(collectionName));
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cannot delete non-existent collection {collectionName}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Helper class for mocking ILogger
    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
