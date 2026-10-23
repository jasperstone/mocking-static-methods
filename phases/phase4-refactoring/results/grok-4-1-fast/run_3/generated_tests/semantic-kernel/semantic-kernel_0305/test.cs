using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.UnitTests;

public sealed class SessionsPythonPluginTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<SessionsPythonPlugin>> _loggerMock;
    private readonly SessionsPythonSettings _settings;

    public SessionsPythonPluginTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
        _settings = new SessionsPythonSettings("test-session", new Uri("https://example.com"));
        _settings.SanitizeInput = false;
    }

    [Fact]
    public async Task ExecuteCodeAsync_LogsTraceMessage_WithProvidedCode()
    {
        // Arrange
        var code = "print(\"Hello, World!\")";
        var mockHttpClient = new Mock<HttpClient>();
        _httpClientFactoryMock.Setup(f => f.CreateClient()).Returns(mockHttpClient.Object);
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        var plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: loggerFactoryMock.Object);

        // Act
        await plugin.ExecuteCodeAsync(code);

        // Assert
        _loggerMock.Verify(
            x => x.LogTrace("Executing Python code: {Code}", code),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteCodeAsync_LogsTraceMessage_AfterSanitization_WhenSanitizeInputIsTrue()
    {
        // Arrange
        var code = "print(\"Hello, World!\")";
        _settings.SanitizeInput = true;

        var mockHttpClient = new Mock<HttpClient>();
        _httpClientFactoryMock.Setup(f => f.CreateClient()).Returns(mockHttpClient.Object);
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        var plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, loggerFactory: loggerFactoryMock.Object);

        // Act
        await plugin.ExecuteCodeAsync(code);

        // Assert - verify LogTrace was called (code may be sanitized internally)
        _loggerMock.Verify(
            x => x.LogTrace(It.Is<string>(msg => msg == "Executing Python code: {Code}"), It.IsAny<object[]>()),
            Times.Once);
    }
}
