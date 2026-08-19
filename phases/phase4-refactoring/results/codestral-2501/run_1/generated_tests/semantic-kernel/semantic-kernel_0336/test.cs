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
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly BraveConnector _connector;

        public BraveConnectorTests()
        {
            _loggerMock = new Mock<ILogger<BraveConnector>>();
            _httpClientMock = new Mock<HttpClient>();
            _connector = new BraveConnector("apiKey", _httpClientMock.Object, null, new NullLoggerFactory());
        }

        [Fact]
        public async Task SearchAsync_LogsTrace()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            var responseContent = new StringContent("{\"web\":{\"results\":[{\"description\":\"test description\"}]}}");
            _httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = responseContent,
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            // Act
            await _connector.SearchAsync<string>(query, count, offset, cancellationToken);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received: {\"web\":{\"results\":[{\"description\":\"test description\"}]}}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
