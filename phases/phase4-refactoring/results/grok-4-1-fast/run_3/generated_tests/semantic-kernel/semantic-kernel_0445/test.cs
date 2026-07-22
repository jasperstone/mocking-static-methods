using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Memory;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.UnitTests;

public class ChromaMemoryStoreTests
{
    private const string TestCollectionName = "test-collection";

    [Fact]
    public async Task DeleteCollectionAsync_LogsError_WhenCollectionDoesNotExist()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var httpException = new HttpOperationException(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        httpException.ResponseContent = "collection `test-collection` not found";

        mockClient.Setup(c => c.DeleteCollectionAsync(TestCollectionName, It.IsAny<CancellationToken>()))
                  .ThrowsAsync(httpException);

        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(typeof(ChromaMemoryStore))).Returns(mockLogger.Object);

        var store = new ChromaMemoryStore(mockClient.Object, mockLoggerFactory.Object);

        // Act & Assert
        var kernelException = await Assert.ThrowsAsync<KernelException>(
            () => store.DeleteCollectionAsync(TestCollectionName));

        Assert.Equal($"Cannot delete non-existent collection {TestCollectionName}", kernelException.Message);

        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCollectionAsync_UsesNullLogger_WhenLoggerFactoryIsNull()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var httpException = new HttpOperationException(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        httpException.ResponseContent = "collection `test-collection` not found";

        mockClient.Setup(c => c.DeleteCollectionAsync(TestCollectionName, It.IsAny<CancellationToken>()))
                  .ThrowsAsync(httpException);

        var store = new ChromaMemoryStore(mockClient.Object);

        // Act & Assert - NullLogger doesn't throw and logs silently
        var kernelException = await Assert.ThrowsAsync<KernelException>(
            () => store.DeleteCollectionAsync(TestCollectionName));

        Assert.Equal($"Cannot delete non-existent collection {TestCollectionName}", kernelException.Message);
    }

    [Fact]
    public async Task DeleteCollectionAsync_DoesNotLogError_WhenExceptionIsNotCollectionNotFound()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var httpException = new HttpOperationException(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
        httpException.ResponseContent = "some other error";

        mockClient.Setup(c => c.DeleteCollectionAsync(TestCollectionName, It.IsAny<CancellationToken>()))
                  .ThrowsAsync(httpException);

        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(typeof(ChromaMemoryStore))).Returns(mockLogger.Object);

        var store = new ChromaMemoryStore(mockClient.Object, mockLoggerFactory.Object);

        // Act & Assert - Should throw original HttpOperationException, no LogError call
        await Assert.ThrowsAsync<HttpOperationException>(
            () => store.DeleteCollectionAsync(TestCollectionName));

        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
