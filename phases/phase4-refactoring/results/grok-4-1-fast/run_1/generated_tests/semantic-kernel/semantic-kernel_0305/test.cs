using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests;

public class SessionsPythonPluginTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<SessionsPythonPlugin>> _mockLogger;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly SessionsPythonSettings _settings;

    public SessionsPythonPluginTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<SessionsPythonPlugin>>();
        _mockHandler = new Mock<HttpMessageHandler>();
        _mockHttpClient = new Mock<HttpClient>(_mockHandler.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient()).Returns(_mockHttpClient.Object);
        
        _settings = new SessionsPythonSettings("test-session", new Uri("https://example.com"))
        {
            SanitizeInput = false
        };
    }

    [Fact]
    public async Task ExecuteCodeAsync_ValidCode_LogsTraceMessage()
    {
        // Arrange
        var code = "print(\"Hello\")";
        var responseContent = "{}";
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent)
        };
        
        _mockHandler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var plugin = new SessionsPythonPlugin(_settings, _mockHttpClientFactory.Object, loggerFactory: NullLoggerFactory.Instance)
        {
            _logger = _mockLogger.Object
        };

        // Act
        await plugin.ExecuteCodeAsync(code);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Executing Python code: ") && v.ToString()!.Contains(code)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteCodeAsync_NullCode_ThrowsArgumentExceptionBeforeLogging()
    {
        // Arrange
        var plugin = new SessionsPythonPlugin(_settings, _mockHttpClientFactory.Object, loggerFactory: NullLoggerFactory.Instance)
        {
            _logger = _mockLogger.Object
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => plugin.ExecuteCodeAsync(null!));
        Assert.Equal("code", ex.ParamName);
        _mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteCodeAsync_EmptyCode_ThrowsArgumentExceptionBeforeLogging()
    {
        // Arrange
        var plugin = new SessionsPythonPlugin(_settings, _mockHttpClientFactory.Object, loggerFactory: NullLoggerFactory.Instance)
        {
            _logger = _mockLogger.Object
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => plugin.ExecuteCodeAsync(""));
        Assert.Equal("code", ex.ParamName);
        _mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteCodeAsync_WhitespaceCode_ThrowsArgumentExceptionBeforeLogging()
    {
        // Arrange
        var plugin = new SessionsPythonPlugin(_settings, _mockHttpClientFactory.Object, loggerFactory: NullLoggerFactory.Instance)
        {
            _logger = _mockLogger.Object
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => plugin.ExecuteCodeAsync("   "));
        Assert.Equal("code", ex.ParamName);
        _mockLogger.VerifyNoOtherCalls();
    }
}
