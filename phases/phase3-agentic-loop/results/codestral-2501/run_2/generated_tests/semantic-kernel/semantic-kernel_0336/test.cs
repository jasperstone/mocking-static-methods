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
            _connector = new BraveConnector("apiKey", _httpClient, new Uri("https://api.search.brave.com/res/v1/web/search?q"), new NullLoggerFactory());
        }

        [Fact]
        public async Task SearchAsync_LogsTrace()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            // Act
            await _connector.SearchAsync<string>(query, count, offset, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received: {Data}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountIsInvalid()
        {
            // Arrange
            var query = "test query";
            var count = 21;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _connector.SearchAsync<string>(query, count, offset, cancellationToken));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetIsInvalid()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 11;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _connector.SearchAsync<string>(query, count, offset, cancellationToken));
        }

        [Fact]
        public async Task SearchAsync_ReturnsExpectedResults()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _connector.SearchAsync<string>(query, count, offset, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<string>>(result);
        }
    }
}
