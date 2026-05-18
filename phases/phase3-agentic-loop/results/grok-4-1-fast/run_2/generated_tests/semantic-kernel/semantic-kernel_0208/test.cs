using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_RegistersServiceCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        string modelId = "gpt-4o-mini";
        var endpoint = new Uri("https://api.example.com/v1");

        // Act
        var result = services.AddOpenAIChatClient(
            modelId: modelId,
            endpoint: endpoint,
            apiKey: "test-key");

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);
        
        var descriptor = services.First();
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.True(descriptor.ImplementationFactory != null);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        string modelId = "gpt-4o-mini";
        var endpoint = new Uri("https://api.example.com/v1");
        string serviceId = "test-service";

        // Act
        services.AddOpenAIChatClient(
            modelId: modelId,
            endpoint: endpoint,
            serviceId: serviceId);

        // Assert
        Assert.Single(services);
        var descriptor = services.First();
        Assert.NotNull(descriptor.ServiceKey);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        string modelId = "gpt-4o-mini";
        var endpoint = new Uri("https://api.example.com/v1");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services!.AddOpenAIChatClient(modelId, endpoint));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_ReturnsSameServicesInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        string modelId = "gpt-4o-mini";
        var endpoint = new Uri("https://api.example.com/v1");

        // Act
        var result = services.AddOpenAIChatClient(modelId, endpoint);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_RegistersSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        string modelId = "gpt-4o-mini";
        var endpoint = new Uri("https://api.example.com/v1");

        // Act
        services.AddOpenAIChatClient(modelId, endpoint);

        // Assert
        var descriptor = Assert.Single(services);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }
}
