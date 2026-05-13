using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_LogsTraceWithResponseContent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

            var httpMessageHandlerMock = new MockHttpMessageHandler();
            var httpClient = new HttpClient(httpMessageHandlerMock);

            var connector = new BraveConnector("fake-api-key", httpClient, loggerFactory: loggerFactoryMock.Object);

            string expectedJson = @"{
                ""Web"": {
                    ""Results"": [
                        {
                            ""Title"": ""Title1"",
                            ""Description"": ""Description1"",
                            ""Url"": ""http://example.com/1""
                        }
                    ]
                }
            }";

            httpMessageHandlerMock.SetResponseContent(expectedJson);

            // Act
            var results = await connector.SearchAsync<string>("test query", 1, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Helper HttpMessageHandler to mock HttpClient responses
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private string _responseContent = "{}";

            public void SetResponseContent(string content)
            {
                _responseContent = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_responseContent)
                };
                return Task.FromResult(response);
            }
        }
    }
}
