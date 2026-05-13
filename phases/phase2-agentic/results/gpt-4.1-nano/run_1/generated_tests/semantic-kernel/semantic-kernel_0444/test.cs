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
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public ChromaClientTests()
        {
            _loggerMock = new Mock<ILogger>();
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object);
        }

        [Fact]
        public async Task ExecuteHttpRequestAsync_Should_LogError_When_HttpOperationExceptionThrown()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var exception = new HttpOperationException("Error message", "ResponseContent");
            var client = new ChromaClient(_httpClient, "http://endpoint", loggerFactory: null);
            // Use reflection to set private _logger to mock
            var loggerField = typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(client, _loggerMock.Object);

            // Setup the SendWithSuccessCheckAsync to throw HttpOperationException
            _handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Throws(exception);

            // Act & Assert
            await Assert.ThrowsAsync<HttpOperationException>(async () =>
            {
                await client.ExecuteHttpRequestAsync(request);
            });

            // Verify that LogError was called with the exception
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("operation failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Mocked exception class for testing
    public class HttpOperationException : Exception
    {
        public string ResponseContent { get; }

        public HttpOperationException(string message, string responseContent) : base(message)
        {
            ResponseContent = responseContent;
        }
    }
}
