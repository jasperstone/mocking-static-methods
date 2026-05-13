using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.Tests
{
    public class SessionsPythonPluginTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly SessionsPythonSettings _settings;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public SessionsPythonPluginTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _settings = new SessionsPythonSettings { Endpoint = new Uri("http://localhost") };
        }

        [Fact]
        public async Task ExecuteCodeAsync_LogsTrace_Call()
        {
            // Arrange
            var plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: null);
            var code = "print(\"hello\")";

            var responseContent = new StringContent(JsonSerializer.Serialize(new SessionsPythonCodeExecutionResult { Output = "hello" }));
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            _httpMessageHandlerMock
                .SetupRequest(HttpMethod.Post, "executions")
                .ReturnsAsync(responseMessage);

            // Act
            await plugin.ExecuteCodeAsync(code);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Extension method for setting up HttpMessageHandler mock
    public static class HttpMessageHandlerExtensions
    {
        public static Mock<HttpMessageHandler> SetupRequest(this Mock<HttpMessageHandler> mock, HttpMethod method, string path)
        {
            mock
                .Setup(m => m.Send(It.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri != null &&
                    req.RequestUri.AbsolutePath.EndsWith(path))))
                .Returns<HttpRequestMessage>(req => Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{}")
                }));
            return mock;
        }

        public static Task<HttpResponseMessage> ReturnsAsync(this Mock<HttpMessageHandler> mock, HttpResponseMessage response)
        {
            mock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(response);
            return Task.CompletedTask;
        }
    }
}
