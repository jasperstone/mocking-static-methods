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
            // We cannot mock ILoggerFactory.CreateLogger because it is an extension method.
            // Instead, we create a BraveConnector with a NullLoggerFactory and replace the _logger field via reflection.
            var httpClient = new HttpClient(new TestHttpMessageHandler(
                "{\"type\":\"search\",\"web\":{\"results\":[{\"type\":\"result\",\"url\":\"http://example.com\",\"title\":\"title\",\"description\":\"desc\",\"age\":\"1d\"}]}}"));

            var braveConnector = new BraveConnector(
                apiKey: "fake-api-key",
                httpClient: httpClient,
                uri: null,
                loggerFactory: null);

            // Use reflection to set the private _logger field to our mock
            var loggerField = typeof(BraveConnector).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(braveConnector, loggerMock.Object);

            // Act
            var results = await braveConnector.SearchAsync<string>("test query", 1, 0, CancellationToken.None);

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

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _responseContent;

            public TestHttpMessageHandler(string responseContent = null)
            {
                _responseContent = responseContent ?? "{\"type\":\"search\"}";
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
