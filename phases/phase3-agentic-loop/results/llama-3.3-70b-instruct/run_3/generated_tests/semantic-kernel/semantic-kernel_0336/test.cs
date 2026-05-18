using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Tests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_ValidQuery_ReturnsResults()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var httpClient = new HttpClient();
            var braveConnector = new BraveConnector("apiKey", new Uri("https://api.search.brave.com/res/v1/web/search?q"), loggerFactory);

            // Act
            var results = await braveConnector.SearchAsync<string>("query", 1, 0, CancellationToken.None);

            // Assert
            Assert.NotNull(results);
            Assert.True(results != null && results.Count > 0);
        }

        [Fact]
        public async Task SearchAsync_InvalidQuery_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var httpClient = new HttpClient();
            var braveConnector = new BraveConnector("apiKey", new Uri("https://api.search.brave.com/res/v1/web/search?q"), loggerFactory);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>(null, 1, 0, CancellationToken.None));
        }

        [Fact]
        public async Task SearchAsync_LogTrace_CallsLogTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BraveConnector>>();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
            var httpClient = new HttpClient();
            var braveConnector = new BraveConnector("apiKey", new Uri("https://api.search.brave.com/res/v1/web/search?q"), loggerFactory);

            // Act
            await braveConnector.SearchAsync<string>("query", 1, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }
    }
}
