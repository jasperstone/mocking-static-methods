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
            var httpClient = new HttpClient(new FakeHttpMessageHandler());

            // We create a BraveConnector with a null logger factory to avoid the extension method call
            var connector = new BraveConnector("fakeApiKey", httpClient, loggerFactory: null);

            // Use reflection to set the private _logger field to our mock logger
            var loggerField = typeof(BraveConnector).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(connector, loggerMock.Object);

            // Act
            var results = await connector.SearchAsync<string>("test query", 1, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var jsonResponse = @"{
                    ""Web"": {
                        ""Results"": [
                            {
                                ""Title"": ""Title1"",
                                ""Description"": ""Description1"",
                                ""Url"": ""http://example.com""
                            }
                        ]
                    }
                }";

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                };

                return Task.FromResult(response);
            }
        }
    }
}
