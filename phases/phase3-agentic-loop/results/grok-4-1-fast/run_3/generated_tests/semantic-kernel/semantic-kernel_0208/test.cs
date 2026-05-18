using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_NoLoggerAvailable_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        services.AddOpenAIChatClient(
            modelId: "gpt-4",
            endpoint: new Uri("https://example.com"),
            apiKey: "test-key");

        var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetService<object>();
    }

    [Fact]
    public void AddOpenAIChatClient_WithLoggerAvailable_SuccessfullyRegisters()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);

        // Act
        services.AddOpenAIChatClient(
            modelId: "gpt-4",
            apiKey: "test-key");

        var serviceProvider = services.BuildServiceProvider();
        
        // Trigger factory execution by resolving IChatClient
        _ = serviceProvider.GetService<IChatClient>();

        // Assert - CreateLogger was called (verifies factory executed and used loggerFactory)
        mockLoggerFactory.Verify(lf => lf.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce());
    }

    [Fact]
    public void AddOpenAIChatClient_ApiKeyVersion_WithLogger_Successful()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());

        // Act & Assert
        services.AddOpenAIChatClient(
            modelId: "gpt-4",
            apiKey: "test-key");

        var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetService<IChatClient>();
    }

    [Fact]
    public void AddOpenAIChatClient_EndpointVersion_HandlesNullLoggerGracefully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOpenAIChatClient(
            modelId: "gpt-4",
            endpoint: new Uri("https://example.com"));

        // Assert - returns services (chainable) and doesn't throw
        Assert.Same(services, result);
    }
}
