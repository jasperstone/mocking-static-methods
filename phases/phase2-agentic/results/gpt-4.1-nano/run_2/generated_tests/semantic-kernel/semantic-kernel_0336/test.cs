using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Plugins.Web.Brave;

namespace BraveConnectorTests
{
    public class SearchAsyncLoggingTests
    {
        [Fact]
        public async Task SearchAsync_Should_Call_LogTrace_With_ResponseContent()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var responseContent = "{\"Web\": {\"Results\": [{\"Title\": \"Title1\", \"Description\": \"Desc1\", \"Url\": \"http://example.com\"}]}}";

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (request) =>
                {
                    return await Task.FromResult(responseMessage);
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var connector = new BraveConnector(
                apiKey: "test-api-key",
                httpClient: httpClient,
                loggerFactory: new LoggerFactory()
            );

            // Replace the logger with our mock
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
