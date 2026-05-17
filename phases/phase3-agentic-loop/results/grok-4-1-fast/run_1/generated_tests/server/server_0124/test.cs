using System;
using Microsoft.Extensions.DependencyInjection;
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
    public void AddAzureServiceBusIntegration_ResolvesEventIntegrationHandlerSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Register all required dependencies for EventIntegrationHandler
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());

        var listenerConfig = new TestListenerConfig
        {
            RoutingKey = "test-key",
            IntegrationType = EventIntegrationType.OrganizationUserCreated
        };

        // Act
        services.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfig);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Successful resolution confirms GetRequiredService calls work
        var handler = serviceProvider.GetService<IEventMessageHandler>("test-key");
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_MissingPublisher_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Missing IEventIntegrationPublisher - this will cause GetRequiredService to fail
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());

        var listenerConfig = new TestListenerConfig { RoutingKey = "test-key" };

        // Act & Assert
        services.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfig);
        var serviceProvider = services.BuildServiceProvider();
        
        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetService<IEventMessageHandler>("test-key"));
        Assert.Contains("Unable to resolve service", exception.Message);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_AllGetRequiredServiceCalls_CanBeResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Register all dependencies needed for both handlers and hosted services
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddSingleton<IAzureServiceBusService>(Mock.Of<IAzureServiceBusService>());
        services.AddSingleton<IIntegrationHandler<TestConfig>>(Mock.Of<IIntegrationHandler<TestConfig>>());

        var listenerConfig = new TestListenerConfig { RoutingKey = "test-key" };

        // Act
        services.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfig);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - All GetRequiredService calls are exercised during resolution
        _ = serviceProvider.GetService<IEventMessageHandler>("test-key"); // line 889+
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.True(hostedServices.Length >= 2);
    }
}

// Minimal implementations to satisfy compiler
public class TestConfig { }

public class TestListenerConfig : IIntegrationListenerConfiguration
{
    public string RoutingKey { get; set; } = string.Empty;
    public EventIntegrationType IntegrationType { get; set; }
    public int EventPrefetchCount => 10;
    public int EventMaxConcurrentCalls => 1;
}
