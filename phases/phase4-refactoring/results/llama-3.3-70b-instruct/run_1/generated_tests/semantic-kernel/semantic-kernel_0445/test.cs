using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task DeleteCollectionAsync_LogsErrorAndThrowsKernelException_WhenCollectionDoesNotExist()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var chromaClientMock = new Mock<IChromaClient>();
            chromaClientMock
                .Setup(c => c.DeleteCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new HttpOperationException("Collection not found"));
            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactory);

            // Act and Assert
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync("non-existent-collection"));
        }
    }
}
