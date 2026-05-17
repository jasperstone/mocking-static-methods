using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests.CodeInterpreter;

public class SessionsPythonPluginTests
{
    [Fact]
    public async Task ExecuteCodeAsync_LogsTraceMessage_WhenCalled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SessionsPythonPlugin>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(f => f.CreateClient()).Returns(new Mock<HttpClient>().Object);
        
        var settings = new SessionsPythonSettings("test-session", new Uri("https://test-endpoint.com"));
        settings.SanitizeInput = false;

        var plugin = new SessionsPythonPlugin(
            settings,
            mockHttpClientFactory.Object,
            loggerFactory: Mock.Of<ILoggerFactory>(lf => lf.CreateLogger(typeof(SessionsPythonPlugin)) == mockLogger.Object));

        var code = "print(\"Hello, World!\")";

        // Act
        await plugin.ExecuteCodeAsync(code, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Executing Python code:") && v.ToString()!.Contains(code)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
