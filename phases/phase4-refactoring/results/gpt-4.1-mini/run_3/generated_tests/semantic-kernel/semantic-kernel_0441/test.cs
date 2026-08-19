using System;
using System.Collections.Generic;
using System.Net.Http;
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
        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var httpClient = new HttpClient(new HttpMessageHandlerStub())
            {
                BaseAddress = new Uri("http://localhost")
            };
            var client = new ChromaClient(httpClient, null, loggerFactoryMock.Object);

            // Setup HttpMessageHandlerStub to return a valid JSON for ChromaEmbeddingsModel
            HttpMessageHandlerStub.ResponseContent = "{\"Embeddings\":[],\"Ids\":[],\"Metadatas\":[],\"Documents\":[]}";

            string testCollectionId = "test-collection";
            string[] testIds = new string[] { "id1", "id2" };

            // Act
            var result = await client.GetEmbeddingsAsync(testCollectionId, testIds);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Getting embeddings from collection with id: {testCollectionId}"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class HttpMessageHandlerStub : HttpMessageHandler
        {
            public static string ResponseContent = "{}";

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(ResponseContent)
                };
                return Task.FromResult(response);
            }
        }
    }
}
