using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.Tests
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_LogsTraceWithCode()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(mockHttpMessageHandler);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var settings = new SessionsPythonSettings("sessionId", new Uri("http://localhost"))
            {
                SanitizeInput = false
            };

            var plugin = new SessionsPythonPlugin(settings, mockHttpClientFactory.Object, null, mockLoggerFactory.Object);

            string testCode = "print(\"Hello World\")";

            // Setup mock response for SendAsync with required properties for deserialization
            string jsonResponse = "{\"status\":\"Succeeded\",\"result\":{\"executionResult\":\"Success\",\"stdout\":\"Output\",\"stderr\":\"\"}}";
            mockHttpMessageHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            });

            // Act
            var result = await plugin.ExecuteCodeAsync(testCode, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(testCode)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Helper mock HttpMessageHandler to intercept HTTP calls
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private HttpResponseMessage _response = new HttpResponseMessage(HttpStatusCode.OK);

            public void SetResponse(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    }
}
