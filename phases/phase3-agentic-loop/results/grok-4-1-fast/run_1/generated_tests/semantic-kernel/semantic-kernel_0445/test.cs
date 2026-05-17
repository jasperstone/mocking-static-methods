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
        var loggerMock = new Mock<ILogger<ChromaMemoryStore>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(ChromaMemoryStore))).Returns(loggerMock.Object);

        var clientMock = new Mock<IChromaClient>();
        var exception = new HttpOperationException(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
        {
            Content = new StringContent("Collection 'test-collection' not found")
        });
        clientMock.Setup(c => c.DeleteCollectionAsync("test-collection", It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var store = new ChromaMemoryStore(clientMock.Object, loggerFactoryMock.Object);

        // Act
        var kernelException = await Assert.ThrowsAsync<KernelException>(() => store.DeleteCollectionAsync("test-collection"));

        // Assert
        Assert.Equal("Cannot delete non-existent collection test-collection", kernelException.Message);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Cannot delete non-existent collection test-collection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_UsesNullLogger_WhenLoggerFactoryIsNull()
    {
        // Arrange & Act
        var clientMock = new Mock<IChromaClient>();
        var store = new ChromaMemoryStore(clientMock.Object, null);

        // Assert - constructor works with null logger factory (uses NullLogger.Instance internally)
        Assert.NotNull(store);
    }
}
