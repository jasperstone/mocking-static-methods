using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ListCollectionsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var chromaClient = new ChromaClient("https://example.com", new LoggerFactory().AddMock(loggerMock.Object));

            // Act
            await chromaClient.ListCollectionsAsync().ToListAsync();

            // Assert
            loggerMock.Verify(l => l.LogDebug("Listing collections"), Times.Once);
        }

        [Fact]
        public async Task UpsertEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var chromaClient = new ChromaClient("https://example.com", new LoggerFactory().AddMock(loggerMock.Object));

            // Act
            await chromaClient.UpsertEmbeddingsAsync("collectionId", new[] { "id1" }, new[] { new ReadOnlyMemory<float>(new float[] { 1.0f }) });

            // Assert
            loggerMock.Verify(l => l.LogDebug("Upserting embeddings to collection with id: {0}", "collectionId"), Times.Once);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var chromaClient = new ChromaClient("https://example.com", new LoggerFactory().AddMock(loggerMock.Object));

            // Act
            await chromaClient.GetEmbeddingsAsync("collectionId", new[] { "id1" });

            // Assert
            loggerMock.Verify(l => l.LogDebug("Getting embeddings from collection with id: {0}", "collectionId"), Times.Once);
        }

        [Fact]
        public async Task DeleteEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var chromaClient = new ChromaClient("https://example.com", new LoggerFactory().AddMock(loggerMock.Object));

            // Act
            await chromaClient.DeleteEmbeddingsAsync("collectionId", new[] { "id1" });

            // Assert
            loggerMock.Verify(l => l.LogDebug("Deleting embeddings from collection with id: {0}", "collectionId"), Times.Once);
        }

        [Fact]
        public async Task QueryEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var chromaClient = new ChromaClient("https://example.com", new LoggerFactory().AddMock(loggerMock.Object));

            // Act
            await chromaClient.QueryEmbeddingsAsync("collectionId", new[] { new ReadOnlyMemory<float>(new float[] { 1.0f }) }, 10);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Query embeddings in collection with id: {0}", "collectionId"), Times.Once);
        }
    }
}
