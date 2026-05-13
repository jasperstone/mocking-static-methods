using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests
{
    public class SessionsPythonPluginTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly SessionsPythonSettings _settings;
        private readonly SessionsPythonPlugin _plugin;

        public SessionsPythonPluginTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger>();
            _settings = new SessionsPythonSettings
            {
                Endpoint = new Uri("https://testendpoint")
            };
            _plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: new LoggerFactory().CreateLogger<SessionsPythonPlugin>());
        }

        [Fact]
        public async Task ExecuteCodeAsync_LogsTraceCall()
        {
            // Arrange
            var code = "print(\"hello\")";
            var responseContent = new SessionsPythonCodeExecutionResult { Output = "hello" };
            var responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(JsonSerializer.Serialize(responseContent))
            };

            var messageHandlerMock = new Mock<HttpMessageHandler>();
            messageHandlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    var response = responseMessage;
                    return response;
                });

            var httpClient = new HttpClient(messageHandlerMock.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            // Act
            var result = await _plugin.ExecuteCodeAsync(code);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            Assert.NotNull(result);
        }
    }
}
