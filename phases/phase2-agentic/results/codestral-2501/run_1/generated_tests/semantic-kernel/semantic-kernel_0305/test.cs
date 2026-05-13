using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.Tests
{
    public class SessionsPythonPluginTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<SessionsPythonPlugin>> _loggerMock;
        private readonly SessionsPythonSettings _settings;
        private readonly SessionsPythonPlugin _plugin;

        public SessionsPythonPluginTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            _settings = new SessionsPythonSettings("test-session", new Uri("http://test-endpoint"));
            _plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, null, new NullLoggerFactory());
        }

        [Fact]
        public async Task ExecuteCodeAsync_LogsTrace()
        {
            // Arrange
            var code = "print(\"Hello, World!\")";
            var cancellationToken = new CancellationToken();

            // Act
            await _plugin.ExecuteCodeAsync(code, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code: print(\"Hello, World!\")")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
