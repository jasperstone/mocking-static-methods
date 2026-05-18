using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;

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
            _settings = new SessionsPythonSettings
            {
                Endpoint = new Uri("https://testendpoint")
            };
            _plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: null);
        }

        [Fact]
        public async Task ExecuteCodeAsync_LogsTraceCall()
        {
            // Arrange
            var code = "print(\"hello\")";

            // Use reflection to set the private _logger field to our mock
            var loggerField = typeof(SessionsPythonPlugin).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(_plugin, _loggerMock.Object);

            // Mock SendAsync to return a dummy response
            var responseContent = new StringContent("{\"dummy\": \"value\"}");
            var responseMessage = new Mock<HttpResponseMessage>();
            responseMessage.Setup(r => r.Content).Returns(responseContent);
            var httpClientMock = new Mock<HttpClient>();
            // Since HttpClient is sealed, we need to mock the factory to return a real HttpClient with a handler
            // For simplicity, we can skip actual HTTP call and focus on verifying LogTrace

            // Act
            await _plugin.ExecuteCodeAsync(code, CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
