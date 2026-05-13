using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientLoggingTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly ChromaClient _client;

        public ChromaClientLoggingTests()
        {
            _mockLogger = new Mock<ILogger>();

            // Setup HttpClient with a handler that returns a successful empty response
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            // Create ChromaClient with injected HttpClient and logger factory that returns our mock logger
            var loggerFactory = new LoggerFactory();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(_mockLogger.Object);

            _client = new ChromaClient(_httpClient, loggerFactory: mockLoggerFactory.Object);
        }

        private void SetupHttpResponse(string responseContent)
        {
            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            string collectionId = "test-collection";
            string[] ids = new[] { "id1", "id2" };
            string jsonResponse = "{\"Ids\":[\"id1\",\"id2\"],\"Embeddings\":[[0.1,0.2],[0.3,0.4]]}";

            SetupHttpResponse(jsonResponse);

            // Act
            var result = await _client.GetEmbeddingsAsync(collectionId, ids);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting embeddings from collection with id:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ListCollectionsAsync_LogsDebugMessage()
        {
            // Arrange
            string jsonResponse = "[{\"Name\":\"collection1\"},{\"Name\":\"collection2\"}]";
            SetupHttpResponse(jsonResponse);

            // Act
            var collections = new List<string>();
            await foreach (var collection in _client.ListCollectionsAsync())
            {
                collections.Add(collection);
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Listing collections")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Contains("collection1", collections);
            Assert.Contains("collection2", collections);
        }

        [Fact]
        public async Task UpsertEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            string collectionId = "test-collection";
            string[] ids = new[] { "id1" };
            ReadOnlyMemory<float>[] embeddings = new[] { new float[] { 0.1f, 0.2f }.AsMemory() };

            SetupHttpResponse("{}");

            // Act
            await _client.UpsertEmbeddingsAsync(collectionId, ids, embeddings);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Upserting embeddings to collection with id:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            string collectionId = "test-collection";
            string[] ids = new[] { "id1" };

            SetupHttpResponse("{}");

            // Act
            await _client.DeleteEmbeddingsAsync(collectionId, ids);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting embeddings from collection with id:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task QueryEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            string collectionId = "test-collection";
            ReadOnlyMemory<float>[] queryEmbeddings = new[] { new float[] { 0.1f, 0.2f }.AsMemory() };
            int nResults = 5;
            string jsonResponse = "{}";

            SetupHttpResponse(jsonResponse);

            // Act
            var result = await _client.QueryEmbeddingsAsync(collectionId, queryEmbeddings, nResults);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Query embeddings in collection with id:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
