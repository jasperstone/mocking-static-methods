using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel;

namespace ChromaMemoryStoreTests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task DeleteCollectionAsync_Should_LogErrorAndThrowKernelException_When_HttpOperationExceptionOccurs()
        {
            // Arrange
            var collectionName = "test-collection";

            var mockClient = new Mock<IChromaClient>();
            var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();

            var exceptionResponseContent = "Collection does not exist";

            mockClient
                .Setup(c => c.DeleteCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpOperationException
                {
                    ResponseContent = exceptionResponseContent
                });

            var store = new ChromaMemoryStore(mockClient.Object, null);
            // Inject the mock logger into the store
            var loggerField = typeof(ChromaMemoryStore).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(store, mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KernelException>(() => store.DeleteCollectionAsync(collectionName));
            Assert.Contains(collectionName, exception.Message);

            // Verify that LogError was called with the expected message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cannot delete non-existent collection {collectionName}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
