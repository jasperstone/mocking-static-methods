using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_LogsTraceMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("https://test"));
            var plugin = new SessionsPythonPlugin(settings, mockHttpClientFactory.Object, null, null);
            // For testing, we need to inject a mock or override SendAsync to avoid real HTTP calls.
            // But for now, focus on verifying the logger call.

            var code = "print(\"Hello\")";

            // Act
            await plugin.ExecuteCodeAsync(code);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Executing Python code: {code}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
