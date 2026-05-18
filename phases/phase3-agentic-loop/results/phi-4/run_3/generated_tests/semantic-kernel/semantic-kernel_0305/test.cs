using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;

public class SessionsPythonPluginTests
{
    [Fact]
    public async Task ExecuteCodeAsync_LogsTraceMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var settings = new SessionsPythonSettings("sessionId", new Uri("https://example.com"))
        {
            SanitizeInput = false
        };

        // Use a constructor that accepts an ILogger
        var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, null, loggerMock.Object);

        string code = "print('Hello, World!')";
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await plugin.ExecuteCodeAsync(code, cancellationToken);

        // Assert
        loggerMock.Verify(
            logger => logger.LogTrace(
                It.Is<string>(message => message == "Executing Python code: {Code}"),
                It.Is<object[]>(args => args[0].ToString() == code)),
            Times.Once);
    }
}
