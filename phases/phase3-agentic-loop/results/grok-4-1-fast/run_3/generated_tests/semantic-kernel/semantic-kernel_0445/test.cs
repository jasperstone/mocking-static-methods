using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Memory;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.UnitTests;

public class ChromaMemoryStoreTests
{
    [Fact]
    public async Task DeleteCollectionAsync_LogsErrorAndThrowsKernelException_WhenCollectionDoesNotExist()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        var mockClient = new Mock<IChromaClient>();
        var httpException = new HttpOperationException("Mock response");
        httpException.ResponseContent = "{\"error\": \"Collection 'test' does not exist\"}"; // Matches VerifyCollectionDoesNotExistMessage condition
        
        mockClient.Setup(c => c.DeleteCollectionAsync("test", It.IsAny<CancellationToken>()))
                  .ThrowsAsync(httpException);
        
        var loggerFactory = new TestLoggerFactory(mockLogger.Object);
        var store = new ChromaMemoryStore(mockClient.Object, loggerFactory);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => store.DeleteCollectionAsync("test"));
        Assert.IsType<KernelException>(exception);
        Assert.Equal("Cannot delete non-existent collection test", exception.Message);
        
        // Verify LogError was called with correct parameters
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
    public async Task DeleteCollectionAsync_SucceedsWithoutLoggingError_WhenCollectionExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChromaMemoryStore>>();
        var mockClient = new Mock<IChromaClient>();
        
        mockClient.Setup(c => c.DeleteCollectionAsync("test", It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);
        
        var loggerFactory = new TestLoggerFactory(mockLogger.Object);
        var store = new ChromaMemoryStore(mockClient.Object, loggerFactory);

        // Act
        await store.DeleteCollectionAsync("test");

        // Assert - no LogError should be called
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

public class TestLoggerFactory : ILoggerFactory
{
    private readonly ILogger _logger;

    public TestLoggerFactory(ILogger logger)
    {
        _logger = logger;
    }

    public void Dispose() { }

    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => _logger;
}
