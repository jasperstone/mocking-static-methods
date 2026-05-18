using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
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
            // We mock ILogger directly, no ILoggerFactory needed to avoid Moq issues with extension methods

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(mockHttpMessageHandler);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var settings = new SessionsPythonSettings("sessionId", new Uri("https://fakeendpoint"))
            {
                SanitizeInput = false
            };

            var plugin = new SessionsPythonPlugin(settings, mockHttpClientFactory.Object, null, loggerFactory: null);
            // We inject the mock logger directly by reflection since constructor expects ILoggerFactory or null
            // But the plugin only uses loggerFactory to create logger, so we can replace the _logger field for testing
            var loggerField = typeof(SessionsPythonPlugin).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(plugin, mockLogger.Object);

            string testCode = "print(\"Hello World\")";

            // Setup the mock HTTP response for the SendAsync call inside ExecuteCodeAsync
            var expectedResult = new SessionsPythonCodeExecutionResult
            {
                Status = "Succeeded",
                Result = new SessionsPythonCodeExecutionResult.ExecutionDetails
                {
                    ExecutionResult = "Hello World",
                    StdOut = "Hello World",
                    StdErr = null
                }
            };
            mockHttpMessageHandler.SetResponse(JsonSerializer.Serialize(expectedResult));

            // Act
            var result = await plugin.ExecuteCodeAsync(testCode);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(testCode)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Succeeded", result.Status);
            Assert.NotNull(result.Result);
            Assert.Equal("Hello World", result.Result.ExecutionResult);
        }

        // Helper class to mock HttpMessageHandler for HttpClient
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private HttpResponseMessage _response = new HttpResponseMessage(HttpStatusCode.OK);

            public void SetResponse(string content)
            {
                _response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content)
                };
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    }
}
