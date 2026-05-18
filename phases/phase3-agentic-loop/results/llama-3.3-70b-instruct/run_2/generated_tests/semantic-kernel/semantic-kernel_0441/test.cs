using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{\"embeddings\": []}")
               });
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com", loggerFactoryMock.Object);
            var collectionId = "test-collection";
            var ids = new[] { "id1", "id2" };
            var include = new[] { "include1", "include2" };

            // Act
            await chromaClient.GetEmbeddingsAsync(collectionId, ids, include);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Getting embeddings from collection with id: {0}", collectionId), Times.Once);
        }

        [Fact]
        public async Task ListCollectionsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("[{\"name\": \"collection1\"}, {\"name\": \"collection2\"}]")
               });
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com", loggerFactoryMock.Object);

            // Act
            await foreach (var _ in chromaClient.ListCollectionsAsync())
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogDebug("Listing collections"), Times.Once);
        }

        [Fact]
        public async Task UpsertEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK
               });
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com", loggerFactoryMock.Object);
            var collectionId = "test-collection";
            var ids = new[] { "id1", "id2" };
            var embeddings = new[] { new ReadOnlyMemory<float>(new float[] { 1.0f, 2.0f }) };
            var metadatas = new object[] { new { key = "value" } };

            // Act
            await chromaClient.UpsertEmbeddingsAsync(collectionId, ids, embeddings, metadatas);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Upserting embeddings to collection with id: {0}", collectionId), Times.Once);
        }

        [Fact]
        public async Task DeleteEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK
               });
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com", loggerFactoryMock.Object);
            var collectionId = "test-collection";
            var ids = new[] { "id1", "id2" };

            // Act
            await chromaClient.DeleteEmbeddingsAsync(collectionId, ids);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Deleting embeddings from collection with id: {0}", collectionId), Times.Once);
        }

        [Fact]
        public async Task QueryEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{\"queryResult\": []}")
               });
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com", loggerFactoryMock.Object);
            var collectionId = "test-collection";
            var queryEmbeddings = new[] { new ReadOnlyMemory<float>(new float[] { 1.0f, 2.0f }) };
            var nResults = 10;
            var include = new[] { "include1", "include2" };

            // Act
            await chromaClient.QueryEmbeddingsAsync(collectionId, queryEmbeddings, nResults, include);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Query embeddings in collection with id: {0}", collectionId), Times.Once);
        }
    }
}
