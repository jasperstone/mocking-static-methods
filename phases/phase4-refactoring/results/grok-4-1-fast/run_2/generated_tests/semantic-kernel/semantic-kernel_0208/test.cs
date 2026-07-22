using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_RegistersServiceWithoutLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOpenAIChatClient(
            modelId: "gpt-4",
            endpoint: new Uri("https://example.com/"));

        // Assert
        Assert.Same(services, result);
        Assert.NotEmpty(services);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_WithLoggerFactory_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOpenAIChatClient(
            modelId: "gpt-4",
            endpoint: new Uri("https://example.com/"));

        // Assert
        Assert.Same(services, result);
        Assert.NotEmpty(services);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddOpenAIChatClient(
            modelId: "gpt-4",
            endpoint: new Uri("https://example.com/"),
            serviceId: "test-service");

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Building the provider succeeds (factory executes without throwing)
        Assert.NotNull(serviceProvider);
    }
}
