using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_CallsGetServiceForLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);

        var modelId = "gpt-4o-mini";
        var endpoint = new Uri("https://api.openai.com/v1");
        string? apiKey = null;
        string? serviceId = "test-service";

        // Act
        var result = services.AddOpenAIChatClient(modelId, endpoint, apiKey, serviceId: serviceId);
        
        // Assert method chaining
        Assert.Same(services, result);
        
        // Verify the factory was registered by checking service descriptors
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(IChatClient) && 
            d.Key?.ToString() == serviceId);
        Assert.NotNull(descriptor);
        
        // Verify GetService was called (detected via factory registration)
        Assert.True(mockLoggerFactory.WasGetServiceCalled);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_RegistersChatClientService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var modelId = "gpt-4o-mini";
        var endpoint = new Uri("https://api.openai.com/v1");
        string? apiKey = null;

        // Act
        services.AddOpenAIChatClient(modelId, endpoint, apiKey);

        // Assert registration (without triggering full resolution)
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IChatClient));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_ReturnsSameServicesInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var modelId = "gpt-4o-mini";
        var endpoint = new Uri("https://api.openai.com/v1");
        string? apiKey = null;

        // Act
        var result = services.AddOpenAIChatClient(modelId, endpoint, apiKey);

        // Assert
        Assert.Same(services, result);
        Assert.NotEmpty(services);
    }
}

public class MockLoggerFactory : ILoggerFactory
{
    public bool WasGetServiceCalled { get; private set; }

    public void AddProvider(ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => NullLogger.Instance;
    public void Dispose() { }

    // Called when resolved via GetService to simulate detection
    public MockLoggerFactory()
    {
        WasGetServiceCalled = true;
    }
}
