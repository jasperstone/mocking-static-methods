using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        private readonly Mock<ILogger<BraveConnector>> _loggerMock;
        private readonly HttpClient _httpClient;
        private readonly BraveConnector _connector;

        public BraveConnectorTests()
        {
            _loggerMock = new Mock<ILogger<BraveConnector>>();
            _httpClient = new HttpClient();
            _connector = new BraveConnector("apiKey", _httpClient, null, NullLoggerFactory.Instance);
        }

        [Fact]
        public async Task SearchAsync_ValidQuery_ReturnsResults()
        {
            // Arrange
            var query = "test";
            var count = 1;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _connector.SearchAsync<BraveWebResult>(query, count, offset, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<BraveWebResult>>(result);
        }

        [Fact]
        public async Task SearchAsync_InvalidCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var query = "test";
            var count = 21;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _connector.SearchAsync<BraveWebResult>(query, count, offset, cancellationToken));
        }

        [Fact]
        public async Task SearchAsync_InvalidOffset_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var query = "test";
            var count = 1;
            var offset = 11;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _connector.SearchAsync<BraveWebResult>(query, count, offset, cancellationToken));
        }

        [Fact]
        public async Task SearchAsync_LogsTrace()
        {
            // Arrange
            var query = "test";
            var count = 1;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            // Act
            await _connector.SearchAsync<BraveWebResult>(query, count, offset, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
