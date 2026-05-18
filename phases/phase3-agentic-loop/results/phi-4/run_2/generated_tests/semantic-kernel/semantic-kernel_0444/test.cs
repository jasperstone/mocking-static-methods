using System;
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
        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsErrorOnHttpOperationException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ChromaClient>>();
            var mockHttpClient = new Mock<HttpClient>();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new MockLoggerProvider(mockLogger.Object)));
            var client = new ChromaClient(mockHttpClient.Object, "http://example.com", loggerFactory);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/api/v1/resource");
            var exception = new HttpOperationException("Test exception", new HttpResponseMessage(), "Response content");

            mockHttpClient
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Test exception", exception));

            // Act & Assert
            await Assert.ThrowsAsync<HttpOperationException>(() => client.ExecuteHttpRequestAsync(request));

            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v is HttpOperationException),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly Mock<ILogger> _logger;

        public MockLoggerProvider(Mock<ILogger> logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger.Object;
        }

        public void Dispose()
        {
        }
    }
}
