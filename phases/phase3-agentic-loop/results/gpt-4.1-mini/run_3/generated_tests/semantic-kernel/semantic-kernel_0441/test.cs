using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Moq.Protected;
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
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            // Use NullLoggerFactory to create a logger that we can mock
            var loggerFactory = NullLoggerFactory.Instance;
            _client = new ChromaClient(_httpClient, loggerFactory: loggerFactory);

            // Replace the private _logger field with our mock using reflection
            var loggerField = typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(_client, _mockLogger.Object);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var collectionId = "testCollection";
            var ids = new[] { "id1", "id2" };

            var responseContent = "{\"Embeddings\":[],\"Ids\":[],\"Metadatas\":[]}";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();

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
    }
}
