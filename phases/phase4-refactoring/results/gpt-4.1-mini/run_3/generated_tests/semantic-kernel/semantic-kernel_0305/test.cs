using System;
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
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            var settings = new SessionsPythonSettings("session1", new Uri("http://localhost"))
            {
                SanitizeInput = false
            };

            // Setup HttpClientFactory to return a HttpClient that returns a dummy response
            var handler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(handler);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var plugin = new SessionsPythonPlugin(settings, mockHttpClientFactory.Object, null, new LoggerFactory());
            var code = "print('hello')";

            // Act
            var result = await plugin.ExecuteCodeAsync(code, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(code)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        // A simple HttpMessageHandler that returns a valid JSON response for SessionsPythonCodeExecutionResult
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Return a JSON string that matches the expected structure of SessionsPythonCodeExecutionResult
                var jsonResponse = "{\"status\":\"Succeeded\",\"result\":{\"executionResult\":\"success\",\"stdout\":\"output\",\"stderr\":\"\"}}";
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                };
                return Task.FromResult(response);
            }
        }
    }
}
