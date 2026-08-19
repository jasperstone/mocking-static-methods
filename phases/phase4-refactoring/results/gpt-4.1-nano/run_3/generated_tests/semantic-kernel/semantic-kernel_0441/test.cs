using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace ChromaClientTests
{
    public class ChromaClientLoggingTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly ChromaClient _client;

        public ChromaClientLoggingTests()
        {
            _loggerMock = new Mock<ILogger>();
            var httpClient = new HttpClient(); // Not used in these tests
            _client = new ChromaClient(httpClient, "http://test", new LoggerFactory().AddProvider(new MockLoggerProvider(_loggerMock.Object)));
        }

        [Fact]
        public async Task ListCollectionsAsync_ShouldLogDebug()
        {
            // Arrange
            var responseContent = "[{\"Name\": \"col1\"}, {\"Name\": \"col2\"}]";

            var mockHttp = new Mock<HttpMessageHandler>();
            mockHttp.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(responseContent)
                });
            var httpClient = new HttpClient(mockHttp.Object);
            var client = new ChromaClient(httpClient, "http://test", new LoggerFactory().AddProvider(new MockLoggerProvider(_loggerMock.Object)));

            // Act
            await foreach (var name in client.ListCollectionsAsync())
            {
                // Consume the async enumerable
            }

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Listing collections")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }

    // Helper classes for mocking
    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
