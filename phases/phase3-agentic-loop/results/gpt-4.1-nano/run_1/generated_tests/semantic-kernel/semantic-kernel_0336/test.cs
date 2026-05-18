using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Plugins.Web.Brave;

namespace BraveConnectorTests
{
    public class BraveConnectorLoggingTests
    {
        [Fact]
        public async Task SearchAsync_LogsTrace_WithResponseContent()
        {
            // Arrange
            var mockHttpClient = new Mock<IHttpClient>();
            var mockLogger = new Mock<ILogger<BraveConnector>>();
            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Web\": {\"Results\": [{\"Title\": \"Title1\", \"Description\": \"Desc1\", \"Url\": \"http://url1\"}]}}")
            };

            mockHttpClient
                .Setup(c => c.SendWithSuccessCheckAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var connector = new BraveConnector(
                apiKey: "test-api-key",
                httpClient: mockHttpClient.Object,
                loggerFactory: loggerFactory.Object
            );

            // Act
            var result = await connector.SearchAsync<string>("test query", count: 1);

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
