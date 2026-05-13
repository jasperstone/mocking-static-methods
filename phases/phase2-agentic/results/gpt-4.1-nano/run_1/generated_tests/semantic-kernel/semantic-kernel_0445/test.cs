using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaMemoryStoreTests
    {
        private readonly Mock<IChromaClient> _mockChromaClient;
        private readonly Mock<ILogger> _mockLogger;
        private readonly ChromaMemoryStore _store;

        public ChromaMemoryStoreTests()
        {
            _mockChromaClient = new Mock<IChromaClient>();
            _mockLogger = new Mock<ILogger>();
            _store = new ChromaMemoryStore(_mockChromaClient.Object, new LoggerFactory().AddProvider(new MockLoggerProvider(_mockLogger.Object)));
        }

        [Fact]
        public async Task DeleteCollectionAsync_WhenHttpOperationException_WhenVerifyCollectionDoesNotExistMessage_ReturnsLogsErrorAndThrows()
        {
            // Arrange
            var collectionName = "testCollection";
            var exceptionMessage = "Collection does not exist";
            var httpException = new HttpOperationException { ResponseContent = exceptionMessage };
            _mockChromaClient
                .Setup(c => c.DeleteCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(httpException);
            _mockChromaClient
                .Setup(c => c.DeleteCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback(() => { throw new HttpOperationException { ResponseContent = exceptionMessage }; });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KernelException>(() => _store.DeleteCollectionAsync(collectionName));
            _mockLogger.Verify(
                x => x.LogError("Cannot delete non-existent collection {0}", collectionName),
                Times.Once);
            Assert.Contains(collectionName, exception.Message);
        }
    }

    // Helper classes for mocking ILogger
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

    // Custom exception to simulate HttpOperationException
    public class HttpOperationException : Exception
    {
        public string ResponseContent { get; set; }
    }
}
