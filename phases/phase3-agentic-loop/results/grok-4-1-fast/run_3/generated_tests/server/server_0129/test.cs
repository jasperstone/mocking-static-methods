using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureServiceBusIntegration_ResolvesEventMessageHandlerWithAllGetRequiredServiceCalls()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all dependencies that GetRequiredService calls in EventIntegrationHandler factory
        services.AddSingleton(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton(Mock.Of<IUserRepository>());
        services.AddSingleton(Mock.Of<IOrganizationRepository>());
        services.AddLogging();

        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.SetupGet(x => x.RoutingKey).Returns("test-key");
        listenerConfig.SetupGet(x => x.IntegrationType).Returns("test");

        // Act
        Bit.SharedWeb.Utilities.ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(
            services, listenerConfig.Object);

        // Assert - Verifies all GetRequiredService calls succeeded
        using var provider = services.BuildServiceProvider();
        var handler = provider.GetKeyedService<IEventMessageHandler>("test-key");
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_ResolvesEventListenerHostedServiceWithGetRequiredServiceCalls()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IAzureServiceBusService>());
        services.AddLogging();

        // Pre-register the keyed handler that GetRequiredKeyedService will resolve
        services.AddKeyedSingleton<IEventMessageHandler>("test-key", Mock.Of<IEventMessageHandler>());

        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.SetupGet(x => x.RoutingKey).Returns("test-key");
        listenerConfig.SetupGet(x => x.EventPrefetchCount).Returns(10);
        listenerConfig.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);
        listenerConfig.SetupGet(x => x.IntegrationType).Returns("test");

        // Act
        Bit.SharedWeb.Utilities.ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(
            services, listenerConfig.Object);

        // Assert - Verifies GetRequiredKeyedService, GetRequiredService<IAzureServiceBusService>, GetRequiredService<ILoggerFactory>
        using var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s.GetType().Name.Contains("AzureServiceBusEventListenerService"));
    }

    [Fact]
    public void AddAzureServiceBusIntegration_ResolvesIntegrationListenerHostedServiceWithGetRequiredServiceCalls()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IAzureServiceBusService>());
        services.AddSingleton<IIntegrationHandler<object>>(Mock.Of<IIntegrationHandler<object>>());
        services.AddLogging();

        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.SetupGet(x => x.IntegrationPrefetchCount).Returns(20);
        listenerConfig.SetupGet(x => x.IntegrationMaxConcurrentCalls).Returns(10);

        // Act
        Bit.SharedWeb.Utilities.ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(
            services, listenerConfig.Object);

        // Assert - Verifies GetRequiredService<IIntegrationHandler>, GetRequiredService<IAzureServiceBusService>, GetRequiredService<ILoggerFactory>
        using var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s.GetType().Name.Contains("AzureServiceBusIntegrationListenerService"));
    }

    [Fact]
    public void AddAzureServiceBusIntegration_GetRequiredServiceLine894_ThrowsWhenLoggerMissing()
    {
        // Arrange - Missing ILogger registration to specifically test line 894 GetRequiredService call
        var services = new ServiceCollection();
        // Don't call AddLogging() - this will cause the ILogger<EventIntegrationHandler<T>> GetRequiredService to fail

        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.SetupGet(x => x.RoutingKey).Returns("fail-key");
        listenerConfig.SetupGet(x => x.IntegrationType).Returns("test");

        // Act
        Bit.SharedWeb.Utilities.ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(
            services, listenerConfig.Object);

        using var provider = services.BuildServiceProvider();
        
        // Assert - Specifically tests the GetRequiredService<ILogger<EventIntegrationHandler<T>>> call on line 894
        var ex = Assert.Throws<InvalidOperationException>(() => provider.GetKeyedService<IEventMessageHandler>("fail-key"));
        Assert.Contains("Unable to resolve service for type", ex.Message);
    }
}
