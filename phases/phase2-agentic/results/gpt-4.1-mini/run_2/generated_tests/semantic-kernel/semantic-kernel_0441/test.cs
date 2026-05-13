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
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(_mockLogger.Object);

            _client = new ChromaClient(_httpClient, loggerFactory: loggerFactory.Object);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var collectionId = "test-collection";
            var ids = new[] { "id1", "id2" };
            var expectedLogMessage = $"Getting embeddings from collection with id: {collectionId}";

            var responseContent = "{\"Ids\":[\"id1\",\"id2\"],\"Embeddings\":[[0.1,0.2],[0.3,0.4]]}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _mockHandler
                .SetupRequest(HttpMethod.Post, "http://localhost/api/v1/embeddings/get")
                .ReturnsAsync(response);

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

    internal static class HttpMessageHandlerExtensions
    {
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(this Mock<HttpMessageHandler> mockHandler, HttpMethod method, string requestUri)
        {
            return mockHandler
                .Setup(m => m.SendAsync(It.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri!.ToString() == requestUri),
                    It.IsAny<CancellationToken>()));
        }

        public static Task<HttpResponseMessage> SendAsync(this HttpMessageHandler handler, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return handler.SendAsync(request, cancellationToken);
        }
    }
}
