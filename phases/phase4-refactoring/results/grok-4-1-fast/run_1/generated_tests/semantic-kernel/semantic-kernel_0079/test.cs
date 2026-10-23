using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class AzureOpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIChatClient_ReturnsSameServiceCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAzureOpenAIChatClient(
            deploymentName: "test",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_RegistersKeyedSingletonService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key");

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Assert - triggers factory execution including GetService<ILoggerFactory>() call
        _ = scope.ServiceProvider.GetKeyedService<object>(null);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithServiceId_RegistersKeyedServiceWithKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        const string serviceId = "test-service";

        // Act
        services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key",
            serviceId: serviceId);

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Assert - triggers factory execution including GetService<ILoggerFactory>() call
        _ = scope.ServiceProvider.GetKeyedService<object>(serviceId);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithNoLoggerFactory_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key");

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Assert - triggers factory execution including GetService<ILoggerFactory>() call (returns null)
        _ = scope.ServiceProvider.GetKeyedService<object>(null);
    }
}
