using System.Threading;
using System.Threading.Tasks;
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
            var mockLogger = new Mock<ILogger>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var settings = new SessionsPythonSettings
            {
                Endpoint = new Uri("https://example.com"),
                SanitizeInput = false
            };

            var plugin = new SessionsPythonPlugin(settings, mockHttpClientFactory.Object, loggerFactory: null)
            {
                _logger = mockLogger.Object
            };

            var code = "print('Hello, World!')";
            var cancellationToken = CancellationToken.None;

            // Act
            await plugin.ExecuteCodeAsync(code, cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogTrace(
                    It.Is<string>(message => message.Contains("Executing Python code:")),
                    It.Is<object[]>(parameters => parameters[0].ToString() == code)),
                Times.Once);
        }
    }
}
