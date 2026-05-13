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
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly Mock<ILogger> _loggerMock;

        public BraveConnectorTests()
        {
            _httpClientMock = new Mock<HttpClient>();
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public async Task SearchAsync_LogsTrace_WhenResponseContentReceived()
        {
            // Arrange
            var jsonResponse = "{\"Web\": {\"Results\": [{\"Title\": \"Title1\", \"Description\": \"Desc1\", \"Url\": \"http://url1\"}]}, \"Videos\": {\"Results\": [{\"Title\": \"Title2\", \"Description\": \"Desc2\", \"Url\": \"http://url2\"}]}}";

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (request) =>
                {
                    return responseMessage;
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var connector = new BraveConnector("apiKey", httpClient, null, new LoggerFactory());

            // Act
            var result = await connector.SearchAsync<string>("test query", 1, 0);

            // Assert
            // Verify that LogTrace was called with the expected message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
