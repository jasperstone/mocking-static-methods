using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Memory;
using Moq;
using Moq.Language.Flow;
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
        var responseContent = "{\"detail\": \"Collection 'test-collection' not found\"}";
        var mockHttpException = new HttpOperationException(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent(responseContent) });

        mockClient
            .Setup(c => c.DeleteCollectionAsync(TestCollectionName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(mockHttpException);

        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(typeof(ChromaMemoryStore))).Returns(mockLogger.Object);

        var store = new ChromaMemoryStore(mockClient.Object, mockLoggerFactory.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KernelException>(
            () => store.DeleteCollectionAsync(TestCollectionName));

        Assert.Equal($"Cannot delete non-existent collection {TestCollectionName}", exception.Message);

        mockClient.Verify(c => c.DeleteCollectionAsync(TestCollectionName, It.IsAny<CancellationToken>()), Times.Once);

        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cannot delete non-existent collection test-collection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCollectionAsync_ThrowsKernelException_WhenCollectionDoesNotExist()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var responseContent = "{\"detail\": \"Collection 'test-collection' not found\"}";
        var mockHttpException = new HttpOperationException(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent(responseContent) });

        mockClient
            .Setup(c => c.DeleteCollectionAsync(TestCollectionName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(mockHttpException);

        var store = new ChromaMemoryStore(mockClient.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KernelException>(
            () => store.DeleteCollectionAsync(TestCollectionName));

        Assert.Equal($"Cannot delete non-existent collection {TestCollectionName}", exception.Message);
        mockClient.Verify(c => c.DeleteCollectionAsync(TestCollectionName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCollectionAsync_Succeeds_WhenCollectionExists()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        mockClient
            .Setup(c => c.DeleteCollectionAsync(TestCollectionName, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var store = new ChromaMemoryStore(mockClient.Object);

        // Act
        await store.DeleteCollectionAsync(TestCollectionName);

        // Assert
        mockClient.Verify(c => c.DeleteCollectionAsync(TestCollectionName, It.IsAny<CancellationToken>()), Times.Once);
    }
}
