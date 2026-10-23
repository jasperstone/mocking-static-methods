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
        [Fact]
        public async Task DeleteCollectionAsync_ThrowsKernelException_WhenCollectionDoesNotExist()
        {
            // Arrange
            var collectionName = "nonexistent-collection";

            var mockClient = new Mock<IChromaClient>();
            mockClient
                .Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpOperationException(
                    new System.Net.Http.HttpResponseMessage(),
                    $"Collection '{collectionName}' does not exist"));

            var logger = new TestLogger();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(logger);

            var store = new ChromaMemoryStore(mockClient.Object, mockLoggerFactory.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KernelException>(() => store.DeleteCollectionAsync(collectionName));

            Assert.Contains(collectionName, ex.Message);
            Assert.Contains($"Cannot delete non-existent collection {collectionName}", logger.LastErrorMessage);
        }

        private class TestLogger : ILogger
        {
            public string? LastErrorMessage { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    LastErrorMessage = formatter(state, exception);
                }
            }

            private class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();
                public void Dispose() { }
            }
        }
    }

    // Minimal stub for HttpOperationException to simulate the exception thrown by the client
    public class HttpOperationException : Exception
    {
        public string ResponseContent { get; }

        public HttpOperationException(System.Net.Http.HttpResponseMessage response, string content)
            : base(content)
        {
            this.ResponseContent = content;
        }
    }

    // Minimal stub for KernelException to match the thrown exception type
    public class KernelException : Exception
    {
        public KernelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
