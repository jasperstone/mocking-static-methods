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
        private readonly Mock<ILogger<BraveConnector>> _mockLogger;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly BraveConnector _braveConnector;

        public BraveConnectorTests()
        {
            _mockLogger = new Mock<ILogger<BraveConnector>>();
            _mockHttpClient = new Mock<HttpClient>();
            _braveConnector = new BraveConnector("apiKey", _mockHttpClient.Object, null, Mock.Of<ILoggerFactory>());
        }

        [Fact]
        public async Task SearchAsync_ValidQuery_ReturnsResults()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            var mockResponse = new HttpResponseMessage
            {
                Content = new StringContent("{\"web\":{\"results\":[{\"description\":\"test description\"}]}}")
            };

            _mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _braveConnector.SearchAsync<string>(query, count, offset, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("test description", result.First());
        }

        [Fact]
        public async Task SearchAsync_InvalidCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var query = "test query";
            var count = 21;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _braveConnector.SearchAsync<string>(query, count, offset, cancellationToken));
        }

        [Fact]
        public async Task SearchAsync_InvalidOffset_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 11;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _braveConnector.SearchAsync<string>(query, count, offset, cancellationToken));
        }

        [Fact]
        public async Task SearchAsync_LogsTrace()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            var mockResponse = new HttpResponseMessage
            {
                Content = new StringContent("{\"web\":{\"results\":[{\"description\":\"test description\"}]}}")
            };

            _mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            await _braveConnector.SearchAsync<string>(query, count, offset, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received: {Data}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
