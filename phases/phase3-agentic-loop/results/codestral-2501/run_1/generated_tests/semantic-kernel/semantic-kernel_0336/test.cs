using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        private readonly Mock<ILogger<BraveConnector>> _loggerMock;
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly BraveConnector _braveConnector;

        public BraveConnectorTests()
        {
            _loggerMock = new Mock<ILogger<BraveConnector>>();
            _httpClientMock = new Mock<HttpClient>();
            _braveConnector = new BraveConnector("apiKey", _httpClientMock.Object, null, NullLoggerFactory.Instance);
        }

        [Fact]
        public async Task SearchAsync_LogsTrace()
        {
            // Arrange
            var query = "test query";
            var count = 1;
            var offset = 0;
            var cancellationToken = CancellationToken.None;

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"web\":{\"results\":[{\"description\":\"test description\"}]}}")
            };

            _httpClientMock.Setup(client => client.SendWithSuccessCheckAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await _braveConnector.SearchAsync<string>(query, count, offset, cancellationToken);

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
    }
}
