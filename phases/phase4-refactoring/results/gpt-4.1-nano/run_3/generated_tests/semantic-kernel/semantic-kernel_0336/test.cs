using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web.Brave;

namespace BraveConnectorTests
{
    public class BraveConnectorLoggingTests
    {
        [Fact]
        public async Task SearchAsync_Should_LogTrace_ResponseContent()
        {
            // Arrange
            var jsonResponse = @"
            {
                ""type"": ""search"",
                ""web"": {
                    ""type"": ""search"",
                    ""results"": [
                        {
                            ""title"": ""Test Title"",
                            ""description"": ""Test Description"",
                            ""url"": ""https://example.com""
                        }
                    ]
                }
            }";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (request) =>
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonResponse)
                    };
                    return await Task.FromResult(responseMessage);
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var loggerMock = new Mock<ILogger<BraveConnector>>();

            var connector = new BraveConnector(
                apiKey: "test-api-key",
                httpClient: httpClient,
                loggerFactory: null);

            // Inject the mock logger
            typeof(BraveConnector)
                .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(connector, loggerMock.Object);

            // Act
            var results = await connector.SearchAsync<string>("test query", count: 1);

            // Assert
            loggerMock.Verify(
                x => x.LogTrace(It.Is<string>(s => s.Contains("Response content received")), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
