using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<HttpClient> _httpClientMock;

        public BraveConnectorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpClientMock = new Mock<HttpClient>();
        }

        [Fact]
        public async Task SearchAsync_LogsResponseContent()
        {
            // Arrange
            var braveConnector = new BraveConnector(string.Empty, _httpClientMock.Object, null, new LoggerFactory().CreateLogger<BraveConnector>());
            var query = "test query";
            var count = 1;
            var offset = 0;
            var cancellationToken = default;

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"Test Title\",\"url\":\"https://www.test.com\",\"description\":\"Test Description\"}]}}")
            };

            _httpClientMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await braveConnector.SearchAsync<string>(query, count, offset, cancellationToken);

            // Assert
            _loggerMock.Verify(l => l.LogTrace("Response content received: {Data}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_ForInvalidCount()
        {
            // Arrange
            var braveConnector = new BraveConnector(string.Empty, _httpClientMock.Object, null, new LoggerFactory().CreateLogger<BraveConnector>());
            var query = "test query";
            var count = 21;
            var offset = 0;
            var cancellationToken = default;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>(query, count, offset, cancellationToken));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_ForInvalidOffset()
        {
            // Arrange
            var braveConnector = new BraveConnector(string.Empty, _httpClientMock.Object, null, new LoggerFactory().CreateLogger<BraveConnector>());
            var query = "test query";
            var count = 1;
            var offset = 11;
            var cancellationToken = default;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>(query, count, offset, cancellationToken));
        }
    }
}
