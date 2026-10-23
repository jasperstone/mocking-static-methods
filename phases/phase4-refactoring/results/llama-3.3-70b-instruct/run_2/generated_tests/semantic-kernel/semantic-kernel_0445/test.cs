using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task DeleteCollectionAsync_LogsError_WhenCollectionDoesNotExist()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var chromaClientMock = new Mock<IChromaClient>();
            var collectionName = "test-collection";
            var cancellationToken = CancellationToken.None;

            chromaClientMock
                .Setup(c => c.DeleteCollectionAsync(collectionName, cancellationToken))
                .Throws(new HttpOperationException("Collection not found"));

            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactory);

            // Act and Assert
            var logger = loggerFactory.CreateLogger<ChromaMemoryStore>();
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync(collectionName, cancellationToken));
            loggerFactory.AssertLogged(logger, LogLevel.Error, "Cannot delete non-existent collection {0}", collectionName);
        }
    }
}
