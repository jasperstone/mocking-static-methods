using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web.Brave;

namespace BraveConnectorTests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_LogsTrace_WithResponseContent()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpHandler = new Mock<HttpMessageHandler>();

            var responseContent = "{\"Web\": {\"Results\": [{\"Title\": \"Title1\", \"Description\": \"Desc1\", \"Url\": \"http://example.com\"}]}}";

            mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);

            var connector = new BraveConnector(
                apiKey: "test-api-key",
                httpClient: httpClient,
                loggerFactory: null);

            // Inject the mock logger
            typeof(BraveConnector).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(connector, mockLogger.Object);

            // Act
            var results = await connector.SearchAsync<string>("test query", count: 1);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
