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
        private readonly Mock<ILogger<ChromaClient>> _loggerMock;
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly ChromaClient _chromaClient;

        public ChromaClientTests()
        {
            _loggerMock = new Mock<ILogger<ChromaClient>>();
            _httpClientMock = new Mock<HttpClient>();
            _chromaClient = new ChromaClient(_httpClientMock.Object, "http://test-endpoint", new LoggerFactory());
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var collectionId = "test-collection";
            var ids = new[] { "id1", "id2" };
            var include = new[] { "include1", "include2" };

            var responseContent = JsonSerializer.Serialize(new ChromaEmbeddingsModel());
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _chromaClient.GetEmbeddingsAsync(collectionId, ids, include);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting embeddings from collection with id: test-collection")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);

            Assert.NotNull(result);
        }
    }
}
