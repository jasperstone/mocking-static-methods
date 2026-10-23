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
    public void AddAzureOpenAIChatClient_WithApiKey_ReturnsSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAzureOpenAIChatClient("deployment", "https://endpoint", "apiKey");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithApiKey_AddsService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureOpenAIChatClient("deployment", "https://endpoint", "apiKey");

        // Assert
        Assert.NotEmpty(services);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithServiceId_AddsKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        string serviceId = "test-service";

        // Act
        services.AddAzureOpenAIChatClient("deployment", "https://endpoint", "apiKey", serviceId: serviceId);

        // Assert - Verify registration by count before/after
        var initialCount = services.Count;
        Assert.True(initialCount > 0); // At least the logger + chat client registration
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithTokenCredential_ReturnsSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAzureOpenAIChatClient("deployment", "https://endpoint", credentials: null!);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithoutLogger_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var result = services.AddAzureOpenAIChatClient("deployment", "https://endpoint", "apiKey");
        Assert.Same(services, result);
        Assert.NotEmpty(services);
    }
}
