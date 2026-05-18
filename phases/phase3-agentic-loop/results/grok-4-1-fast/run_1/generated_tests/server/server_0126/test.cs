using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureServiceBusIntegration_RegistersEventMessageHandlerKey()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupRequiredServices(services);
        
        var listenerConfig = Mock.Of<IIntegrationListenerConfiguration>(c =>
            c.RoutingKey == "test-key" &&
            c.IntegrationType == IntegrationType.Webhook);

        // Act
        services.AddAzureServiceBusIntegration<IntegrationConfiguration, IIntegrationListenerConfiguration>(listenerConfig);

        // Assert - Verify registration by checking service descriptors
        var descriptors = services.Where(d => 
            d.ServiceType == typeof(IEventMessageHandler) && 
            d.Key?.ToString() == "test-key").ToList();
        
        Assert.Single(descriptors);
        Assert.Equal(ServiceLifetime.Singleton, descriptors[0].Lifetime);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_RegistersHostedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupRequiredServices(services);
        
        var listenerConfig = Mock.Of<IIntegrationListenerConfiguration>();

        // Act
        services.AddAzureServiceBusIntegration<IntegrationConfiguration, IIntegrationListenerConfiguration>(listenerConfig);

        // Assert
        var eventListenerDescriptors = services.Where(d =>
            d.ServiceType == typeof(IEnumerable<IHostedService>) &&
            d.ImplementationFactory != null &&
            d.ImplementationFactory.Target?.GetType().Name.Contains("AzureServiceBusEventListenerService") == true
        ).ToList();
        
        var integrationListenerDescriptors = services.Where(d =>
            d.ServiceType == typeof(IEnumerable<IHostedService>) &&
            d.ImplementationFactory != null &&
            d.ImplementationFactory.Target?.GetType().Name.Contains("AzureServiceBusIntegrationListenerService") == true
        ).ToList();

        Assert.Single(eventListenerDescriptors);
        Assert.Single(integrationListenerDescriptors);
    }

    private static void SetupRequiredServices(IServiceCollection services)
    {
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddSingleton<IAzureServiceBusService>(Mock.Of<IAzureServiceBusService>());
        services.AddSingleton<IIntegrationHandler<IntegrationConfiguration>>(Mock.Of<IIntegrationHandler<IntegrationConfiguration>>());
        
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger<EventIntegrationHandler<IntegrationConfiguration>>>());
        services.AddSingleton(loggerFactory.Object);
    }
}
