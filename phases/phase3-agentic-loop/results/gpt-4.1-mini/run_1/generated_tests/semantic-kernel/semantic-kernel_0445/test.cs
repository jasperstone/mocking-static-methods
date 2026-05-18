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

            // Setup CreateLogger to return the mockLogger for any string input
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockClient = new Mock<IChromaClient>();

            // Setup DeleteCollectionAsync to throw HttpOperationException with ResponseContent indicating collection does not exist
            var exception = new HttpOperationException("Not Found")
            {
                ResponseContent = $"Collection '{collectionName}' does not exist."
            };

            mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var store = new ChromaMemoryStore(mockClient.Object, mockLoggerFactory.Object);

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
    }

    // Minimal stub for HttpOperationException to simulate the exception thrown by the client
    public class HttpOperationException : Exception
    {
        public string? ResponseContent { get; set; }

        public HttpOperationException(string message) : base(message)
        {
        }
    }

    // Minimal stub for KernelException to match the thrown exception in ChromaMemoryStore
    public class KernelException : Exception
    {
        public KernelException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
