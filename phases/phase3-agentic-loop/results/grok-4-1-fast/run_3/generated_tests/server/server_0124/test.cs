using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.Enums;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureServiceBusIntegration_CallsGetRequiredService_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all required services including the missing IIntegrationHandler<TConfig>
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddLogging();
        services.AddSingleton<IAzureServiceBusService>(Mock.Of<IAzureServiceBusService>());
        
        // Register IIntegrationHandler<TConfig> which was missing
        services.AddSingleton<IIntegrationHandler<TestConfig>>(Mock.Of<IIntegrationHandler<TestConfig>>());
        
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(c => c.IntegrationType).Returns(IntegrationType.Scim);
        listenerConfig.Setup(c => c.RoutingKey).Returns("test-key");
        listenerConfig.Setup(c => c.IntegrationPrefetchCount).Returns(10);
        listenerConfig.Setup(c => c.IntegrationMaxConcurrentCalls).Returns(5);

        // Act
        services.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfig.Object);

        // Build provider and trigger resolution
        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();

        // Assert - no exceptions thrown
        Assert.NotEmpty(hostedServices);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_ThrowsInvalidOperationException_WhenEventPublisherMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddLogging();
        services.AddSingleton<IAzureServiceBusService>(Mock.Of<IAzureServiceBusService>());
        services.AddSingleton<IIntegrationHandler<TestConfig>>(Mock.Of<IIntegrationHandler<TestConfig>>());
        
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(c => c.IntegrationType).Returns(IntegrationType.Scim);
        listenerConfig.Setup(c => c.RoutingKey).Returns("test-key");

        // Act
        services.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfig.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - throws when IEventIntegrationPublisher is missing (GetRequiredService call on line 889)
        Assert.ThrowsAny<InvalidOperationException>(() => 
            serviceProvider.GetServices<IHostedService>().ToList());
    }

    [Fact]
    public void AddAzureServiceBusIntegration_ThrowsInvalidOperationException_WhenIntegrationHandlerMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddLogging();
        services.AddSingleton<IAzureServiceBusService>(Mock.Of<IAzureServiceBusService>());
        
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(c => c.IntegrationType).Returns(IntegrationType.Scim);
        listenerConfig.Setup(c => c.RoutingKey).Returns("test-key");

        // Act
        services.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfig.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - throws when IIntegrationHandler<TConfig> is missing
        Assert.ThrowsAny<InvalidOperationException>(() => 
            serviceProvider.GetServices<IHostedService>().ToList());
    }
}

// Test classes
public class TestConfig { }

public class TestListenerConfig : IIntegrationListenerConfiguration
{
    public IntegrationType IntegrationType { get; set; } = IntegrationType.Scim;
    public string IntegrationQueueName { get; set; } = "";
    public string IntegrationRetryQueueName { get; set; } = "";
    public string IntegrationSubscriptionName { get; set; } = "";
    public string IntegrationTopicName { get; set; } = "";
    public int MaxRetries { get; set; } = 3;
    public int IntegrationPrefetchCount { get; set; } = 10;
    public int IntegrationMaxConcurrentCalls { get; set; } = 5;
    
    string IEventListenerConfiguration.EventPrefetchCount => IntegrationPrefetchCount.ToString();
    string IEventListenerConfiguration.EventMaxConcurrentCalls => IntegrationMaxConcurrentCalls.ToString();
    
    public string RoutingKey => IntegrationType.ToRoutingKey();
}
