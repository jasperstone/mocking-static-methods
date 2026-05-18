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
            // Instead of mocking ILoggerFactory.CreateLogger (which is an extension method),
            // we create BraveConnector with a null loggerFactory and replace the private _logger field via reflection.
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"web\":{\"results\":[{\"description\":\"desc1\",\"title\":\"title1\",\"url\":\"http://url1\"}]}}")
            };
            var httpClient = new HttpClient(new TestHttpMessageHandler(httpResponse));
            var braveConnector = new BraveConnector("fake-api-key", httpClient, loggerFactory: null);

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
            private readonly HttpResponseMessage _response;

            public TestHttpMessageHandler(HttpResponseMessage? response = null)
            {
                _response = response ?? new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"web\":{\"results\":[{\"description\":\"desc1\",\"title\":\"title1\",\"url\":\"http://url1\"}]}}")
                };
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    }
}
