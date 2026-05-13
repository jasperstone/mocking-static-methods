using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;

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
            var mockHttpClient = new Mock<HttpClient>(MockBehavior.Strict, new HttpMessageHandlerStub());
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient.Object);

            var settings = new SessionsPythonSettings
            {
                Endpoint = new Uri("https://fakeendpoint"),
                SanitizeInput = false
            };

            var plugin = new SessionsPythonPlugin(
                settings,
                mockHttpClientFactory.Object,
                null,
                new LoggerFactoryStub(mockLogger.Object));

            var code = "print(\"Hello World\")";

            // Setup SendAsync to return a successful response with a JSON body
            var responseContent = JsonSerializer.Serialize(new SessionsPythonCodeExecutionResult { Result = "Success" });
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            // Replace SendAsync with a delegate that returns the prepared response
            plugin.SetSendAsyncFunc((client, method, url, token, content) =>
            {
                return Task.FromResult(httpResponse);
            });

            // Act
            var result = await plugin.ExecuteCodeAsync(code);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(code)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Success", result.Result);
        }

        // Helper classes and methods for testing

        private class HttpMessageHandlerStub : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });
            }
        }

        private class LoggerFactoryStub : ILoggerFactory
        {
            private readonly ILogger _logger;

            public LoggerFactoryStub(ILogger logger)
            {
                _logger = logger;
            }

            public void AddProvider(ILoggerProvider provider) { }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }
    }

    // Extension to allow injecting SendAsync delegate for testing
    internal static class SessionsPythonPluginTestExtensions
    {
        public static void SetSendAsyncFunc(this SessionsPythonPlugin plugin, Func<HttpClient, HttpMethod, string, CancellationToken, HttpContent?, Task<HttpResponseMessage>> func)
        {
            var field = typeof(SessionsPythonPlugin).GetField("_sendAsyncFunc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                // Add a private field _sendAsyncFunc to SessionsPythonPlugin for testing or use reflection to replace SendAsync method
                throw new InvalidOperationException("Cannot find _sendAsyncFunc field for testing.");
            }
            field.SetValue(plugin, func);
        }
    }
}
