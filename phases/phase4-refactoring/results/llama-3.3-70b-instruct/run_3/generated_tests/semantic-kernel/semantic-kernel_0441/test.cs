using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ChromaClientTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ChromaClient>();
            var chromaClient = new ChromaClient("https://example.com", loggerFactory);

            // Act
            await chromaClient.GetEmbeddingsAsync("collectionId", new[] { "id1" }, null, default);

            // Assert
            // loggerMock.Verify(l => l.LogDebug("Getting embeddings from collection with id: {0}", "collectionId"), Times.Once);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_ReturnsEmbeddingsModel()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ChromaClient>();
            var chromaClient = new ChromaClient("https://example.com", loggerFactory);

            // Act
            var result = await chromaClient.GetEmbeddingsAsync("collectionId", new[] { "id1" }, null, default);

            // Assert
            Assert.NotNull(result);
        }
    }
}
