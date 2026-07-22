using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaMemoryStoreTests
    {
        private class TestLoggerFactory : ILoggerFactory
        {
            public ILogger Logger { get; }

            public TestLoggerFactory(ILogger logger)
            {
                this.Logger = logger;
            }

            public void AddProvider(ILoggerProvider provider) { }

            public ILogger CreateLogger(string categoryName) => this.Logger;

            public void Dispose() { }
        }

        [Fact]
        public async Task DeleteCollectionAsync_LogsErrorAndThrows_WhenCollectionDoesNotExist()
        {
            // Arrange
            var collectionName = "nonexistent-collection";

            var mockClient = new Mock<IChromaClient>();
            mockClient
                .Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpOperationException
                {
                    ResponseContent = $"Collection '{collectionName}' does not exist."
                });

            var mockLogger = new Mock<ILogger>();
            var loggerFactory = new TestLoggerFactory(mockLogger.Object);

            var store = new ChromaMemoryStore(mockClient.Object, loggerFactory);

            // Act
            KernelException? thrownException = null;
            try
            {
                await store.DeleteCollectionAsync(collectionName);
            }
            catch (KernelException ex)
            {
                thrownException = ex;
            }

            // Assert
            Assert.NotNull(thrownException);
            Assert.Contains(collectionName, thrownException.Message);
            Assert.IsType<HttpOperationException>(thrownException.InnerException);

            mockLogger.Verify(
                x => x.LogError("Cannot delete non-existent collection {0}", collectionName),
                Times.Once);
        }
    }

    // Minimal stub for HttpOperationException to simulate the exception thrown by IChromaClient
    public class HttpOperationException : Exception
    {
        public string ResponseContent { get; set; } = string.Empty;
    }

    // Minimal stub for KernelException to simulate the exception thrown by ChromaMemoryStore
    public class KernelException : Exception
    {
        public KernelException(string message, Exception innerException) : base(message, innerException) { }
    }
}
