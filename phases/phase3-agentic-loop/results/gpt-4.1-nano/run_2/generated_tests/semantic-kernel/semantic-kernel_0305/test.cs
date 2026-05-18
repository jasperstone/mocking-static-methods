using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Text;
using System.IO;

namespace PluginTests
{
    public class SessionsPythonPluginTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly SessionsPythonSettings _settings;
        private readonly SessionsPythonPlugin _plugin;

        public SessionsPythonPluginTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _settings = new SessionsPythonSettings("sessionId", new Uri("http://localhost"));

            _plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: null);
        }

        [Fact]
        public async Task ExecuteCodeAsync_LogsTraceCall()
        {
            // Arrange
            var code = "print(\"hello\")";

            var plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: null);
            var loggerField = typeof(SessionsPythonPlugin).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(plugin, _loggerMock.Object);

            // Mock HttpClient to return a dummy response
            var responseContent = new SessionsPythonCodeExecutionResult
            {
                Status = "Succeeded",
                Result = new SessionsPythonCodeExecutionResult.ExecutionDetails
                {
                    StdOut = "hello",
                    StdErr = "",
                    ExecutionResult = "success"
                }
            };
            var jsonResponse = JsonSerializer.Serialize(responseContent);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient
                .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Setup factory to return the mock HttpClient
            _httpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(new HttpClient(new FakeHttpMessageHandler(responseMessage)));

            // Act
            await plugin.ExecuteCodeAsync(code, CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }

    // Helper class to mock HttpClient behavior
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
