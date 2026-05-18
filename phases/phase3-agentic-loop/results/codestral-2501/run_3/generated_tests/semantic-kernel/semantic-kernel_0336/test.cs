using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        private readonly Mock<ILogger<BraveConnector>> _loggerMock;
        private readonly HttpClient _httpClient;
        private readonly BraveConnector _braveConnector;

        public BraveConnectorTests()
        {
            _loggerMock = new Mock<ILogger<BraveConnector>>();
            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

            _httpClient = new HttpClient();
            _braveConnector = new BraveConnector("apiKey", _httpClient, null, loggerFactory.Object);
        }

        [Fact]
        public async Task SearchAsync_LogsTrace()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{\"web\":{\"results\":[{\"description\":\"test\"}]}}"),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector("apiKey", httpClient, null, new NullLoggerFactory());

            // Act
            await braveConnector.SearchAsync<string>(query, count, offset, cancellationToken);

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
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountIsLessThanOrEqualToZero()
        {
            // Arrange
            var query = "test query";
            var count = 0;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _braveConnector.SearchAsync<string>(query, count, offset, cancellationToken));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountIsGreaterThanOrEqualTo21()
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
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetIsLessThanZero()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = -1;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _braveConnector.SearchAsync<string>(query, count, offset, cancellationToken));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetIsGreaterThan10()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 11;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _braveConnector.SearchAsync<string>(query, count, offset, cancellationToken));
        }
    }
}
