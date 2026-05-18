using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core;
using Bit.Core.Repositories;
using Bit.Core.HostedServices;
using Azure.Messaging.ServiceBus;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureServiceBusIntegration_RegistersEventMessageHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all required dependencies for GetRequiredService calls
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);

        var listenerConfig = Mock.Of<IIntegrationListenerConfiguration>(c => 
            c.RoutingKey == "test-key" && 
            c.IntegrationType == EventIntegrationType.Example);

        // Act
        Bit.SharedWeb.Utilities.ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(
            services, listenerConfig);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetKeyedService<IEventMessageHandler>("test-key");
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_RegistersEventListenerService()
    {
        // Arrange
        var services = new ServiceCollection();
        
        services.AddSingleton<IAzureServiceBusService>(Mock.Of<IAzureServiceBusService>());
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);

        var listenerConfig = Mock.Of<IIntegrationListenerConfiguration>(c => 
            c.RoutingKey == "test-key" && 
            c.EventPrefetchCount == 10 &&
            c.EventMaxConcurrentCalls == 5 &&
            c.IntegrationType == EventIntegrationType.Example);

        // Register handler dependency first
        services.AddKeyedSingleton<IEventMessageHandler>("test-key", Mock.Of<IEventMessageHandler>());

        // Act
        Bit.SharedWeb.Utilities.ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(
            services, listenerConfig);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s.GetType().Name.Contains("AzureServiceBusEventListenerService"));
    }

    [Fact]
    public void AddAzureServiceBusIntegration_RegistersIntegrationListenerService()
    {
        // Arrange
        var services = new ServiceCollection();
        
        services.AddSingleton<IAzureServiceBusService>(Mock.Of<IAzureServiceBusService>());
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);

        // Register required handler dependency
        services.AddSingleton<IIntegrationHandler<object>>(Mock.Of<IIntegrationHandler<object>>());

        var listenerConfig = Mock.Of<IIntegrationListenerConfiguration>(c => 
            c.IntegrationPrefetchCount == 20 &&
            c.IntegrationMaxConcurrentCalls == 3);

        // Act
        Bit.SharedWeb.Utilities.ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(
            services, listenerConfig);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s.GetType().Name.Contains("AzureServiceBusIntegrationListenerService"));
    }
}
