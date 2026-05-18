using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
            var braveConnector = new BraveConnector("apiKey", null, loggerFactory);

            // Act
            var results = await braveConnector.SearchAsync<string>("query", 1, 0, default);

            // Assert
            Assert.NotNull(results);
            Assert.Single(results);
        }

        [Fact]
        public async Task SearchAsync_InvalidQuery_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var httpClient = new HttpClient();
            var braveConnector = new BraveConnector("apiKey", null, loggerFactory);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>(null, 1, 0, default));
        }

        [Fact]
        public async Task SearchAsync_LogTrace_CallsLogTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BraveConnector>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var braveConnector = new BraveConnector("apiKey", null, loggerFactoryMock.Object);

            // Act
            await braveConnector.SearchAsync<string>("query", 1, 0, default);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
