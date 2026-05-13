using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.Tests
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_LogsTraceMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionsPythonPlugin>>();
            var settings = new SessionsPythonSettings
            {
                Endpoint = "https://example.com",
                SanitizeInput = false
            };
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var plugin = new SessionsPythonPlugin(settings, httpClientFactory.Object, null, null)
            {
                _logger = mockLogger.Object
            };

            string code = "print('Hello, World!')";
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            await plugin.ExecuteCodeAsync(code, cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogTrace(
                    It.Is<string>(s => s.Contains("Executing Python code:")),
                    It.Is<object[]>(o => o[0].ToString() == code)),
                Times.Once);
        }
    }
}
