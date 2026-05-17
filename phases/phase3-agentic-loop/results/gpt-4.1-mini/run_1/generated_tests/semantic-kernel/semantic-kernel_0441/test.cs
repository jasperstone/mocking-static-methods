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
    public class ChromaClientLoggingTests
    {
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public TestHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var collectionId = "test-collection";
            var ids = new[] { "id1", "id2" };
            string[]? include = null;

            var embeddingsModel = new ChromaEmbeddingsModel();
            var responseContent = JsonSerializer.Serialize(embeddingsModel);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            var handler = new TestHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            // Create client with logger injected via derived class
            var client = new ChromaClientForTest(httpClient, loggerMock.Object);

            // Act
            var result = await client.GetEmbeddingsAsync(collectionId, ids, include);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Getting embeddings from collection with id: {collectionId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Derived class to inject ILogger directly, bypassing ILoggerFactory
        private class ChromaClientForTest : ChromaClient
        {
            public ChromaClientForTest(HttpClient httpClient, ILogger logger)
                : base(httpClient, loggerFactory: null)
            {
                this._logger = logger;
            }

            // Override the logger field with the injected mock
            public new ILogger _logger;
        }
    }
}
