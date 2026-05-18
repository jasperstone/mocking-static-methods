using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAIChatClient_WithEndpoint_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var modelId = "gpt-4";
        var endpoint = new Uri("https://example.com/");

        // Act
        var result = services.AddOpenAIChatClient(modelId, endpoint);

        // Assert
        Assert.Same(services, result);
        
        // Verify service registration was added
        var descriptor = services.FirstOrDefault(d => d.ServiceType.Name == "IChatClient");
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor!.Lifetime);
    }

    [Fact]
    public void AddOpenAIChatClient_WithApiKey_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var modelId = "gpt-4";
        var apiKey = "test-key";

        // Act
        var result = services.AddOpenAIChatClient(modelId, apiKey);

        // Assert
        Assert.Same(services, result);
        
        var descriptor = services.FirstOrDefault(d => d.ServiceType.Name == "IChatClient");
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddOpenAIChatClient_WithOpenAIClientOverload_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var modelId = "gpt-4";

        // Act
        var result = services.AddOpenAIChatClient(modelId);

        // Assert
        Assert.Same(services, result);
        
        var descriptor = services.FirstOrDefault(d => d.ServiceType.Name == "IChatClient");
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddOpenAIChatClient_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var modelId = "gpt-4";
        var serviceId = "test-service";
        var endpoint = new Uri("https://example.com/");

        // Act
        var result = services.AddOpenAIChatClient(modelId, endpoint, serviceId: serviceId);

        // Assert
        Assert.Same(services, result);
        
        var descriptor = services.FirstOrDefault(d => d.ServiceType.Name == "IChatClient");
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor!.Lifetime);
    }

    [Fact]
    public void AddOpenAIChatClient_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var modelId = "gpt-4";
        var endpoint = new Uri("https://example.com/");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)services!).AddOpenAIChatClient(modelId, endpoint));
    }
}
