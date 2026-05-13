using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;

        public ChromaClientTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
            _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(_mockLogger.Object);

            // Setup HttpClient with a mock handler to intercept HTTP calls
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var collectionId = "test-collection";
            var ids = new[] { "id1", "id2" };
            var expectedLogMessage = $"Getting embeddings from collection with id: {collectionId}";

            var responseModel = new ChromaEmbeddingsModel();
            var responseContent = JsonSerializer.Serialize(responseModel);

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _mockHttpMessageHandler
                .Setup(handler => handler.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(httpResponse);

            // Setup SendAsync to intercept the HTTP request and return the mocked response
            _mockHttpMessageHandler
                .Setup(handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            var client = new ChromaClient(_httpClient, loggerFactory: _mockLoggerFactory.Object);

            // Act
            var result = await client.GetEmbeddingsAsync(collectionId, ids);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedLogMessage)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListCollectionsAsync_LogsDebugMessage()
        {
            // Arrange
            var expectedLogMessage = "Listing collections";

            var collections = new List<ChromaCollectionModel>
            {
                new ChromaCollectionModel { Name = "collection1" },
                new ChromaCollectionModel { Name = "collection2" }
            };
            var responseContent = JsonSerializer.Serialize(collections);

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _mockHttpMessageHandler
                .Setup(handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            var client = new ChromaClient(_httpClient, loggerFactory: _mockLoggerFactory.Object);

            // Act
            var result = new List<string>();
            await foreach (var collectionName in client.ListCollectionsAsync())
            {
                result.Add(collectionName);
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedLogMessage)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Equal(2, result.Count);
            Assert.Contains("collection1", result);
            Assert.Contains("collection2", result);
        }
    }
}
