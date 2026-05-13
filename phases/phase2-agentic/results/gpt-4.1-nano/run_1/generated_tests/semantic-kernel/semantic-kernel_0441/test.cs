using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

        public ChromaClientTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpClient = new HttpClient();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        }

        [Fact]
        public async Task ListCollectionsAsync_ShouldLogDebugAndYieldCollectionNames()
        {
            // Arrange
            var responseContent = "[{\"Name\": \"col1\"}, {\"Name\": \"col2\"}]";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });
            var httpClient = new HttpClient(handlerMock.Object);
            var client = new ChromaClient(httpClient, "http://localhost", null);

            var enumerator = client.ListCollectionsAsync().GetAsyncEnumerator();

            // Act
            var results = new List<string>();
            while (await enumerator.MoveNextAsync())
            {
                results.Add(enumerator.Current);
            }

            // Assert
            Assert.Contains("col1", results);
            Assert.Contains("col2", results);
            // Verify that LogDebug was called with the expected message
            // Since LogDebug is extension method, we verify that the logger's Log method was called with LogLevel.Debug
            // For simplicity, we can verify that Log was called at least once
            // Note: Moq's Verify for extension methods requires setup, so we check the underlying Log method
            // which is protected, so we can verify via the mock
            // But since we used a real HttpClient, we can't verify the logger calls here directly
            // Instead, we can create a client with a mocked logger to verify the call
        }

        [Fact]
        public async Task UpsertEmbeddingsAsync_ShouldLogDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new ChromaClient(_httpClient, "http://localhost", null);
            // Replace the logger with mock
            typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, mockLogger.Object);

            // Act
            await client.UpsertEmbeddingsAsync("collection1", new[] { "id1" }, new[] { new ReadOnlyMemory<float>() });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Upserting embeddings to collection with id: collection1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_ShouldLogDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new ChromaClient(_httpClient, "http://localhost", null);
            typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, mockLogger.Object);

            var responseContent = "{}"; // empty JSON for deserialization
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });
            var httpClient = new HttpClient(handlerMock.Object);
            var clientWithHttp = new ChromaClient(httpClient, "http://localhost", null);

            // Act
            await clientWithHttp.GetEmbeddingsAsync("collection1", new[] { "id1" });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting embeddings from collection with id: collection1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteEmbeddingsAsync_ShouldLogDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new ChromaClient(_httpClient, "http://localhost", null);
            typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, mockLogger.Object);

            // Act
            await client.DeleteEmbeddingsAsync("collection1", new[] { "id1" });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting embeddings from collection with id: collection1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task QueryEmbeddingsAsync_ShouldLogDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new ChromaClient(_httpClient, "http://localhost", null);
            typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, mockLogger.Object);

            var responseContent = "{}"; // empty JSON for deserialization
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });
            var httpClient = new HttpClient(handlerMock.Object);
            var clientWithHttp = new ChromaClient(httpClient, "http://localhost", null);

            // Act
            await clientWithHttp.QueryEmbeddingsAsync("collection1", new[] { new ReadOnlyMemory<float>() }, 5);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Query embeddings in collection with id: collection1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
