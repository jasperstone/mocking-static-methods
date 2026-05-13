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
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public ChromaClientTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        }

        [Fact]
        public async Task ExecuteHttpRequestAsync_ShouldLogError_WhenHttpOperationExceptionThrown()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var exception = new HttpOperationException("Error message")
            {
                ResponseContent = "Error response"
            };

            var client = new ChromaClient(_httpClient, "http://endpoint", new LoggerFactory().AddConsole());
            var clientType = typeof(ChromaClient);
            var method = clientType.GetMethod("ExecuteHttpRequestAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Setup the mock to throw HttpOperationException
            _httpMessageHandlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Throws(exception);

            // Act & Assert
            await Assert.ThrowsAsync<HttpOperationException>(async () =>
            {
                await client.GetType()
                    .GetMethod("ExecuteHttpRequestAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(client, new object[] { request, CancellationToken.None });
            });

            // Verify that LogError was called with the exception
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("operation failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
