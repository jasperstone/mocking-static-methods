using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests;

public class SessionsPythonPluginTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<SessionsPythonPlugin>> _loggerMock;
    private readonly SessionsPythonSettings _settings;

    public SessionsPythonPluginTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
        _loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        _settings = new SessionsPythonSettings("test-session", new Uri("https://example.com"));
    }

    [Fact]
    public async Task ExecuteCodeAsync_LogsTraceMessage_WithValidCode()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        _httpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClientMock.Object);

        var plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: Mock.Of<ILoggerFactory>(f => f.CreateLogger(typeof(SessionsPythonPlugin)) == _loggerMock.Object));
        
        var code = "print(\"Hello\")";
        var cancellationToken = CancellationToken.None;

        // Act
        await plugin.ExecuteCodeAsync(code, cancellationToken);

        // Assert
        _loggerMock.Verify(
            logger => logger.LogTrace(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "Executing Python code: {Code}",
                code),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteCodeAsync_LogsTraceMessage_WithSanitizedCode()
    {
        // Arrange
        _settings.SanitizeInput = true;
        var httpClientMock = new Mock<HttpClient>();
        _httpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClientMock.Object);

        var plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: Mock.Of<ILoggerFactory>(f => f.CreateLogger(typeof(SessionsPythonPlugin)) == _loggerMock.Object));

        var code = "print(\"Hello\")";
        var cancellationToken = CancellationToken.None;

        // Act
        await plugin.ExecuteCodeAsync(code, cancellationToken);

        // Assert
        _loggerMock.Verify(
            logger => logger.LogTrace(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "Executing Python code: {Code}",
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteCodeAsync_DoesNotLogTrace_WhenCodeIsNull()
    {
        // Arrange
        var plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: Mock.Of<ILoggerFactory>(f => f.CreateLogger(typeof(SessionsPythonPlugin)) == _loggerMock.Object));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => plugin.ExecuteCodeAsync(null!));
        _loggerMock.Verify(l => l.LogTrace(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteCodeAsync_DoesNotLogTrace_WhenCodeIsWhitespace()
    {
        // Arrange
        var plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: Mock.Of<ILoggerFactory>(f => f.CreateLogger(typeof(SessionsPythonPlugin)) == _loggerMock.Object));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => plugin.ExecuteCodeAsync("   "));
        _loggerMock.Verify(l => l.LogTrace(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }
}
