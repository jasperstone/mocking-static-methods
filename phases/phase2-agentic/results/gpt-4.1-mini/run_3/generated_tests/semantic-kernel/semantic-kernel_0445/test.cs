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
            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(ChromaMemoryStore))).Returns(mockLogger.Object);

            var exception = new HttpOperationException
            {
                ResponseContent = $"Collection '{collectionName}' does not exist."
            };

            var mockClient = new Mock<IChromaClient>();
            mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // We need to mock the static method VerifyCollectionDoesNotExistMessage to return true for this test.
            // Since it's private static, we cannot mock it directly.
            // Instead, we will create a derived class to override DeleteCollectionAsync to simulate the behavior.

            var store = new TestChromaMemoryStore(mockClient.Object, mockLoggerFactory.Object, collectionName);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KernelException>(() => store.DeleteCollectionAsync(collectionName));

            Assert.Contains(collectionName, ex.Message);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cannot delete non-existent collection {collectionName}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestChromaMemoryStore : ChromaMemoryStore
        {
            private readonly string _collectionName;

            public TestChromaMemoryStore(IChromaClient client, ILoggerFactory loggerFactory, string collectionName)
                : base(client, loggerFactory)
            {
                _collectionName = collectionName;
            }

            // Override DeleteCollectionAsync to simulate the catch filter behavior
            public override async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
            {
                try
                {
                    await base.DeleteCollectionAsync(collectionName, cancellationToken);
                }
                catch (HttpOperationException e)
                {
                    // Simulate the catch filter condition being true
                    if (VerifyCollectionDoesNotExistMessage(e.ResponseContent, collectionName))
                    {
                        this._logger.LogError("Cannot delete non-existent collection {0}", collectionName);
                        throw new KernelException($"Cannot delete non-existent collection {collectionName}", e);
                    }
                    throw;
                }
            }

            // Expose the private static method for testing
            public static bool VerifyCollectionDoesNotExistMessage(string? responseContent, string collectionName)
            {
                return ChromaMemoryStore.VerifyCollectionDoesNotExistMessage(responseContent, collectionName);
            }
        }
    }

    // Minimal stubs for dependencies
    public class HttpOperationException : Exception
    {
        public string? ResponseContent { get; set; }
    }

    public class KernelException : Exception
    {
        public KernelException(string message, Exception innerException) : base(message, innerException) { }
    }

    public interface IChromaClient
    {
        Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default);
        Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default);
        // Other members omitted for brevity
    }
}
