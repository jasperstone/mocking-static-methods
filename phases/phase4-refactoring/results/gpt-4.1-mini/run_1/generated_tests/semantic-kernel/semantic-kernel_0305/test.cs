using System;
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
            var mockHttpClientFactory = new Mock<System.Net.Http.IHttpClientFactory>();
            var settings = new SessionsPythonSettings("session1", new Uri("http://localhost"))
            {
                SanitizeInput = false
            };

            // Setup HttpClientFactory to return a HttpClient that returns a dummy response
            var handler = new TestHttpMessageHandler();
            var httpClient = new System.Net.Http.HttpClient(handler);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var plugin = new SessionsPythonPlugin(settings, mockHttpClientFactory.Object, null, new LoggerFactory());
            // Replace the logger with our mock
            typeof(SessionsPythonPlugin)
                .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(plugin, mockLogger.Object);

            string code = "print('hello world')";

            // Act
            var result = await plugin.ExecuteCodeAsync(code, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(code)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Succeeded", result.Status);
            Assert.NotNull(result.Result);
            Assert.Equal("Hello, world!", result.Result.ExecutionResult);
        }

        // A simple HttpMessageHandler that returns a JSON matching the expected SessionsPythonCodeExecutionResult structure
        private class TestHttpMessageHandler : System.Net.Http.HttpMessageHandler
        {
            protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Return a JSON that matches the expected structure of SessionsPythonCodeExecutionResult
                string jsonResponse = @"{
                    ""status"": ""Succeeded"",
                    ""result"": {
                        ""executionResult"": ""Hello, world!"",
                        ""stdout"": null,
                        ""stderr"": null
                    }
                }";

                var response = new System.Net.Http.HttpResponseMessage(System.Net.Http.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent(jsonResponse)
                };
                return Task.FromResult(response);
            }
        }
    }
}
