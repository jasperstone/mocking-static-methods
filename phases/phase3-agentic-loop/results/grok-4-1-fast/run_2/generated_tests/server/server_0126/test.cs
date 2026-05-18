using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Repositories;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureServiceBusIntegration_RegistersKeyedServiceSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all required dependencies using Moq
        var mockPublisher = new Mock<IEventIntegrationPublisher>();
        var mockFilter = new Mock<IIntegrationFilterService>();
        var mockConfigCache = new Mock<IIntegrationConfigurationDetailsCache>();
        var mockUserRepo = new Mock<IUserRepository>();
        var mockOrgRepo = new Mock<IOrganizationRepository>();
        var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockServiceBus = new Mock<IAzureServiceBusService>();
        
        services.AddSingleton(mockPublisher.Object);
        services.AddSingleton(mockFilter.Object);
        services.AddSingleton(mockConfigCache.Object);
        services.AddSingleton(mockUserRepo.Object);
        services.AddSingleton(mockOrgRepo.Object);
        services.AddSingleton(mockLogger.Object);
        services.AddSingleton(mockLoggerFactory.Object);
        services.AddSingleton(mockServiceBus.Object);
        
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(x => x.RoutingKey).Returns("test-key");
        listenerConfig.Setup(x => x.IntegrationType).Returns("test");
        listenerConfig.Setup(x => x.EventPrefetchCount).Returns(10);
        listenerConfig.Setup(x => x.EventMaxConcurrentCalls).Returns(5);
        listenerConfig.Setup(x => x.IntegrationPrefetchCount).Returns(10);
        listenerConfig.Setup(x => x.IntegrationMaxConcurrentCalls).Returns(5);

        // Act
        services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify GetRequiredService calls succeed by resolving the service
        var keyedHandler = serviceProvider.GetRequiredKeyedService<IEventMessageHandler>("test-key");
        Assert.NotNull(keyedHandler);
        
        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();
        Assert.True(hostedServices.Count >= 2);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_MissingRequiredService_ThrowsInvalidOperation()
    {
        // Arrange
        var services = new ServiceCollection();
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(x => x.RoutingKey).Returns("test-key");

        // Act
        services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Missing IEventIntegrationPublisher should cause GetRequiredService to throw
        Assert.Throws<InvalidOperationException>(() => 
            serviceProvider.GetRequiredKeyedService<IEventMessageHandler>("test-key"));
    }
}
