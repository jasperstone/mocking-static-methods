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
        public async Task DeleteCollectionAsync_LogsErrorAndThrowsKernelException_WhenCollectionDoesNotExist()
        {
            // Arrange
            var collectionName = "nonexistent-collection";

            var mockClient = new Mock<IChromaClient>();
            // Setup DeleteCollectionAsync to throw HttpOperationException with ResponseContent indicating collection does not exist
            var httpOpException = new HttpOperationException("Not Found")
            {
                ResponseContent = $"Collection '{collectionName}' does not exist."
            };
            mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(httpOpException);

            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(ChromaMemoryStore))).Returns(mockLogger.Object);

            var store = new ChromaMemoryStore(mockClient.Object, mockLoggerFactory.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KernelException>(() => store.DeleteCollectionAsync(collectionName));

            Assert.Contains(collectionName, ex.Message);

            // Verify LogError was called with expected message and collectionName
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Cannot delete non-existent collection {collectionName}"),
                    httpOpException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
