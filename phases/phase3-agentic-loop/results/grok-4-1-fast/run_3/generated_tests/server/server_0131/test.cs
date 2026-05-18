using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Enums;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureServiceBusIntegration_SuccessfullyRegistersServicesWithGetRequiredServiceCalls()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new Mock<ILoggerFactory>().Object);
        services.AddSingleton<IEventIntegrationPublisher>(new Mock<IEventIntegrationPublisher>().Object);
        services.AddSingleton<IIntegrationFilterService>(new Mock<IIntegrationFilterService>().Object);
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(new Mock<IIntegrationConfigurationDetailsCache>().Object);
        services.AddSingleton<IUserRepository>(new Mock<IUserRepository>().Object);
        services.AddSingleton<IOrganizationRepository>(new Mock<IOrganizationRepository>().Object);
        services.AddSingleton<IAzureServiceBusService>(new Mock<IAzureServiceBusService>().Object);

        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(x => x.RoutingKey).Returns("test-key");
        listenerConfig.Setup(x => x.IntegrationType).Returns(IntegrationType.Scim);
        listenerConfig.Setup(x => x.EventPrefetchCount).Returns(10);
        listenerConfig.Setup(x => x.EventMaxConcurrentCalls).Returns(5);
        listenerConfig.Setup(x => x.IntegrationPrefetchCount).Returns(20);
        listenerConfig.Setup(x => x.IntegrationMaxConcurrentCalls).Returns(10);

        // Act
        var result = services.AddAzureServiceBusIntegration<object, Mock<IIntegrationListenerConfiguration>.Object>(listenerConfig.Object);

        // Assert
        Assert.NotNull(result);
        var sp = result.BuildServiceProvider();
        var hostedServices = sp.GetServices<IHostedService>().ToArray();
        Assert.True(hostedServices.Length >= 2);
    }

    [Fact]
    public void AddEventIntegrationServices_RegistersCacheServiceWithGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        var globalSettings = new GlobalSettings();

        // Act
        var result = services.AddEventIntegrationServices(globalSettings);

        // Assert
        Assert.NotNull(result);
        var sp = result.BuildServiceProvider();
        var cacheService = Assert.IsType<IntegrationConfigurationDetailsCacheService>(
            sp.GetRequiredService<IIntegrationConfigurationDetailsCache>());
        var hostedServices = sp.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s == cacheService);
    }
}
