using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_Should_LogTrace_When_ResponseContentReceived()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var responseContent = "{\"Web\": {\"Results\": [{\"Title\": \"Title1\", \"Description\": \"Desc1\", \"Url\": \"http://url1\"}]}}";

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new MockLoggerProvider(mockLogger.Object));

            var connector = new BraveConnector(
                apiKey: "test-api-key",
                httpClient: httpClient,
                loggerFactory: loggerFactory
            );

            // Act
            var results = await connector.SearchAsync<string>("test query", count: 1);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Helper class to inject mock ILogger into LoggerFactory
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
