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
    [Fact]
    public async Task DeleteCollectionAsync_LogsError_WhenCollectionDoesNotExist()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(typeof(ChromaMemoryStore))).Returns(mockLogger.Object);
        
        var store = new ChromaMemoryStore(mockClient.Object, loggerFactory.Object);
        var collectionName = "test-collection";
        
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        response.Content = new StringContent("{\"detail\": \"Collection 'test-collection' not found\"}");
        var httpException = new HttpOperationException(response);

        mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                  .ThrowsAsync(httpException);

        // Act
        var exception = await Assert.ThrowsAsync<KernelException>(
            () => store.DeleteCollectionAsync(collectionName));

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object? state) => 
                    state?.ToString() == $"Cannot delete non-existent collection {collectionName}"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.Equal($"Cannot delete non-existent collection {collectionName}", exception.Message);
    }

    [Fact]
    public async Task DeleteCollectionAsync_ThrowsKernelException_WhenCollectionDoesNotExist()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var loggerFactory = NullLoggerFactory.Instance;
        var store = new ChromaMemoryStore(mockClient.Object, loggerFactory);
        var collectionName = "nonexistent-collection";
        
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        response.Content = new StringContent("{\"detail\": \"Collection 'nonexistent-collection' not found\"}");
        var httpException = new HttpOperationException(response);

        mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                  .ThrowsAsync(httpException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KernelException>(
            () => store.DeleteCollectionAsync(collectionName));

        Assert.Equal($"Cannot delete non-existent collection {collectionName}", exception.Message);
    }

    [Fact]
    public void Constructor_UsesNullLogger_WhenLoggerFactoryIsNull()
    {
        // Arrange & Act
        var store = new ChromaMemoryStore(new Mock<IChromaClient>().Object, null);

        // Assert
        Assert.NotNull(store);
    }
}
