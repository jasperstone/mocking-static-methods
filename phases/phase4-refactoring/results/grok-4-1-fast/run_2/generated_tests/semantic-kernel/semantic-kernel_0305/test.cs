using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests;

public class SessionsPythonPluginTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _mockHttpClient;
    private readonly Mock<ILogger<SessionsPythonPlugin>> _loggerMock;
    private readonly SessionsPythonSettings _settings;
    private readonly IServiceProvider _serviceProvider;

    public SessionsPythonPluginTests()
    {
        _settings = new SessionsPythonSettings("test-session", new Uri("https://test-endpoint"));
        _settings.SanitizeInput = false;

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _mockHttpClient = new HttpClient(_httpMessageHandlerMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton<IHttpClientFactory>(new DefaultHttpClientFactory(_mockHttpClient));
        services.AddLogging(builder => builder.AddProvider(new MockLoggerProvider(_loggerMock = new Mock<ILogger<SessionsPythonPlugin>>())));

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteCodeAsync_ValidCode_LogsTraceMessage()
    {
        // Arrange
        var code = "print(\"hello\")";
        var responseContent = JsonSerializer.Serialize(new { });
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        using var scope = _serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<SessionsPythonPlugin>();

        var plugin = new SessionsPythonPlugin(_settings, httpClientFactory, loggerFactory: loggerFactory);

        // Act
        await plugin.ExecuteCodeAsync(code);

        // Assert - Verify LogTrace was called with correct message template and code
        Mock.Get(logger).Verify(
            l => l.LogTrace("Executing Python code: {Code}", code),
            Times.Once());
    }

    [Fact]
    public async Task ExecuteCodeAsync_WithSanitization_LogsTraceMessage()
    {
        // Arrange
        _settings.SanitizeInput = true;
        var code = "print(\"hello\")";
        var responseContent = JsonSerializer.Serialize(new { });
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        using var scope = _serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        var plugin = new SessionsPythonPlugin(_settings, httpClientFactory, loggerFactory: loggerFactory);

        // Act
        await plugin.ExecuteCodeAsync(code);

        // Assert - Verify LogTrace was called
        var logger = loggerFactory.CreateLogger<SessionsPythonPlugin>();
        Mock.Get(logger).Verify(
            l => l.LogTrace(It.Is<string>(msg => msg.Contains("{Code}")), It.IsAny<object[]>()),
            Times.Once());
    }

    [Fact]
    public async Task ExecuteCodeAsync_NullCode_ThrowsArgumentNullException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var plugin = new SessionsPythonPlugin(_settings, httpClientFactory);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => plugin.ExecuteCodeAsync(null!));
        Assert.Equal("code", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteCodeAsync_EmptyCode_ThrowsArgumentNullException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var plugin = new SessionsPythonPlugin(_settings, httpClientFactory);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => plugin.ExecuteCodeAsync(""));
        Assert.Equal("code", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteCodeAsync_WhitespaceCode_ThrowsArgumentNullException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var plugin = new SessionsPythonPlugin(_settings, httpClientFactory);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => plugin.ExecuteCodeAsync("   "));
        Assert.Equal("code", exception.ParamName);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            });
    }

    private class DefaultHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;
        public DefaultHttpClientFactory(HttpClient client) => _client = client;
        public HttpClient CreateClient(string? name = null) => _client;
    }

    private class MockLoggerProvider : ILoggerProvider
    {
        private readonly Mock<ILogger<SessionsPythonPlugin>> _logger;
        public MockLoggerProvider(Mock<ILogger<SessionsPythonPlugin>> logger) => _logger = logger;
        public ILogger CreateLogger(string categoryName) => _logger.Object;
        public void Dispose() { }
    }
}
