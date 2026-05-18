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
        public async Task DeleteCollectionAsync_LogsErrorAndThrows_WhenCollectionDoesNotExist()
        {
            // Arrange
            var collectionName = "nonexistent-collection";

            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Setup CreateLogger with string parameter to avoid extension method issue
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockClient = new Mock<IChromaClient>();

            var exception = new HttpOperationException
            {
                ResponseContent = $"Collection '{collectionName}' does not exist."
            };

            mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Create ChromaMemoryStore with mocked client and logger factory
            var store = new ChromaMemoryStoreForTest(mockClient.Object, mockLoggerFactory.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KernelException>(() => store.DeleteCollectionAsync(collectionName));

            Assert.Contains(collectionName, ex.Message);

            // Verify that LogError was called with the expected message and collectionName
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cannot delete non-existent collection") && v.ToString()!.Contains(collectionName)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Derived class to override logger creation to use string parameter instead of Type
        private class ChromaMemoryStoreForTest : ChromaMemoryStore
        {
            public ChromaMemoryStoreForTest(IChromaClient client, ILoggerFactory loggerFactory)
                : base(client, loggerFactory)
            {
            }

            protected override ILogger CreateLogger(ILoggerFactory loggerFactory)
            {
                // Use CreateLogger(string) to avoid Moq issues with extension method CreateLogger(Type)
                return loggerFactory.CreateLogger("ChromaMemoryStore");
            }
        }
    }

    // Minimal stub for HttpOperationException to simulate the exception thrown by IChromaClient
    public class HttpOperationException : Exception
    {
        public string? ResponseContent { get; set; }
    }

    // Minimal stub for KernelException to match the thrown exception type
    public class KernelException : Exception
    {
        public KernelException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
