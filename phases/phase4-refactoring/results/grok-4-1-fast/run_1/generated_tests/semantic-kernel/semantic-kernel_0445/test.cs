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
    [Fact]
    public async Task DeleteCollectionAsync_LogsError_WhenCollectionDoesNotExist()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new MockLoggerProvider(mockLogger.Object)));
        
        var store = new ChromaMemoryStore(mockClient.Object, loggerFactory);

        var collectionName = "test-collection";
        var httpException = new HttpOperationException()
        {
            ResponseContent = "{\"detail\": \"Collection 'test-collection' not found\"}"
        };

        mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                  .ThrowsAsync(httpException);

        // Act
        var exception = await Assert.ThrowsAsync<KernelException>(
            () => store.DeleteCollectionAsync(collectionName));

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cannot delete non-existent collection") && v.ToString()!.Contains(collectionName)),
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
        var store = new ChromaMemoryStore(mockClient.Object);

        var collectionName = "nonexistent-collection";
        var httpException = new HttpOperationException()
        {
            ResponseContent = "{\"detail\": \"Collection 'nonexistent-collection' not found\"}"
        };

        mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                  .ThrowsAsync(httpException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KernelException>(
            () => store.DeleteCollectionAsync(collectionName));

        Assert.Equal($"Cannot delete non-existent collection {collectionName}", exception.Message);
    }

    [Fact]
    public async Task DeleteCollectionAsync_Succeeds_WhenCollectionExists()
    {
        // Arrange
        var mockClient = new Mock<IChromaClient>();
        var store = new ChromaMemoryStore(mockClient.Object);

        var collectionName = "existing-collection";
        mockClient.Setup(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        // Act
        await store.DeleteCollectionAsync(collectionName);

        // Assert
        mockClient.Verify(c => c.DeleteCollectionAsync(collectionName, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class MockLoggerProvider : ILoggerProvider
{
    private readonly Mock<ILogger> _logger;

    public MockLoggerProvider(Mock<ILogger> logger)
    {
        _logger = logger;
    }

    public ILogger CreateLogger(string categoryName) => _logger.Object;

    public void Dispose() { }
}
