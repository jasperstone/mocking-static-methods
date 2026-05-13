using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace ChromaClientTests
{
    public class ChromaClientTests
    {
        private readonly Mock<ILogger<ChromaClient>> _mockLogger;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly ChromaClient _chromaClient;

        public ChromaClientTests()
        {
            _mockLogger = new Mock<ILogger<ChromaClient>>();
            _mockHttpClient = new Mock<HttpClient>();
            _chromaClient = new ChromaClient(_mockHttpClient.Object, "http://test-endpoint", new NullLoggerFactory());
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var collectionId = "test-collection";
            var ids = new[] { "id1", "id2" };
            var include = new[] { "include1", "include2" };

            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new ChromaEmbeddingsModel()))
            };

            _mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            await _chromaClient.GetEmbeddingsAsync(collectionId, ids, include);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting embeddings from collection with id: test-collection")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
