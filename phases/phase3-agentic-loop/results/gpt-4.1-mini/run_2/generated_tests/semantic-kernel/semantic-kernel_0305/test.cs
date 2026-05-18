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
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(mockLogger.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var handler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(handler);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var settings = new SessionsPythonSettings("session1", new Uri("https://example.com"))
            {
                SanitizeInput = false
            };

            var plugin = new SessionsPythonPlugin(settings, mockHttpClientFactory.Object, null, mockLoggerFactory.Object);

            // Act
            var code = "print('hello')";
            var result = await plugin.ExecuteCodeAsync(code);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.Equal("Success", result.Result.ExecutionResult);
        }

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var json = "{\"status\":\"Succeeded\",\"result\":{\"executionResult\":\"Success\"}}";
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                };
                return Task.FromResult(response);
            }
        }
    }
}
