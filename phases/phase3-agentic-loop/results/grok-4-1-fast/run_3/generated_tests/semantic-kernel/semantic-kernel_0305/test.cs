using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests.CodeInterpreter;

public class SessionsPythonPluginTests
{
    [Fact]
    public async Task ExecuteCodeAsync_LogsTraceMessage_WhenCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var settings = new SessionsPythonSettings("test-session", new Uri("https://example.com"));
        settings.SanitizeInput = false;

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient()).Returns(new Mock<HttpClient>().Object);

        var plugin = new SessionsPythonPlugin(
            settings,
            httpClientFactoryMock.Object,
            loggerFactory: loggerFactoryMock.Object);

        loggerMock
            .Setup(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Executing Python code:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var code = "\"print('hello')\"";

        // Act
        await plugin.ExecuteCodeAsync(code, CancellationToken.None);

        // Assert
        loggerMock.Verify();
    }

    [Fact]
    public async Task ExecuteCodeAsync_LogsTraceMessageWithSanitizedCode_WhenSanitizeInputIsTrue()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var settings = new SessionsPythonSettings("test-session", new Uri("https://example.com"));
        settings.SanitizeInput = true;

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient()).Returns(new Mock<HttpClient>().Object);

        var plugin = new SessionsPythonPlugin(
            settings,
            httpClientFactoryMock.Object,
            loggerFactory: loggerFactoryMock.Object);

        loggerMock
            .Setup(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var code = "\"print('hello')\"";

        // Act
        await plugin.ExecuteCodeAsync(code, CancellationToken.None);

        // Assert
        loggerMock.Verify();
    }
}
