using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    public interface IIntegrationListenerConfiguration
    {
        string RoutingKey { get; }
        string IntegrationType { get; }
    }

    public class MockListenerConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey { get; set; } = string.Empty;
        public string IntegrationType { get; set; } = string.Empty;
    }

    private class MockRabbitMqService : IRabbitMqService { }
    private class MockEventIntegrationPublisher : IEventIntegrationPublisher { }
    private class MockIntegrationFilterService : IIntegrationFilterService { }
    private class MockConfigCache : IIntegrationConfigurationDetailsCache { }
    private class MockUserRepository : IUserRepository { }
    private class MockOrganizationRepository : IOrganizationRepository { }
    private class MockTimeProvider : TimeProvider { }

    private class MockLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) => Mock.Of<ILogger>();
        public void Dispose() { }
    }

    [Fact]
    public void AddRabbitMqIntegration_ThrowsInvalidOperationException_WhenGetRequiredServiceFails()
    {
        // Arrange
        var services = new ServiceCollection();
        var listenerConfig = new MockListenerConfig { RoutingKey = "test-key", IntegrationType = "Test" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            services.AddRabbitMqIntegration<object, MockListenerConfig>(listenerConfig));
        Assert.Contains("GetRequiredService", exception.Message);
    }

    [Fact]
    public void AddRabbitMqIntegration_Succeeds_WhenAllRequiredServicesRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());
        services.AddSingleton<IRabbitMqService>(new MockRabbitMqService());
        services.AddSingleton<IEventIntegrationPublisher>(new MockEventIntegrationPublisher());
        services.AddSingleton<IIntegrationFilterService>(new MockIntegrationFilterService());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(new MockConfigCache());
        services.AddSingleton<IUserRepository>(new MockUserRepository());
        services.AddSingleton<IOrganizationRepository>(new MockOrganizationRepository());
        services.AddSingleton<TimeProvider>(new MockTimeProvider());
        services.AddLogging();

        var listenerConfig = new MockListenerConfig { RoutingKey = "test-key", IntegrationType = "Test" };

        // Act
        var result = services.AddRabbitMqIntegration<object, MockListenerConfig>(listenerConfig);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddRabbitMqIntegration_RegistersRabbitMqEventListenerService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());
        services.AddSingleton<IRabbitMqService>(new MockRabbitMqService());
        services.AddSingleton<IEventIntegrationPublisher>(new MockEventIntegrationPublisher());
        services.AddSingleton<IIntegrationFilterService>(new MockIntegrationFilterService());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(new MockConfigCache());
        services.AddSingleton<IUserRepository>(new MockUserRepository());
        services.AddSingleton<IOrganizationRepository>(new MockOrganizationRepository());
        services.AddLogging();

        var listenerConfig = new MockListenerConfig { RoutingKey = "test-key", IntegrationType = "Test" };
        services.AddRabbitMqIntegration<object, MockListenerConfig>(listenerConfig);

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();

        // Assert
        Assert.Contains(hostedServices, s => s.GetType().Name.Contains("RabbitMqEventListenerService"));
    }

    [Fact]
    public void AddRabbitMqIntegration_RegistersRabbitMqIntegrationListenerService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());
        services.AddSingleton<IRabbitMqService>(new MockRabbitMqService());
        services.AddSingleton<IEventIntegrationPublisher>(new MockEventIntegrationPublisher());
        services.AddSingleton<IIntegrationFilterService>(new MockIntegrationFilterService());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(new MockConfigCache());
        services.AddSingleton<IUserRepository>(new MockUserRepository());
        services.AddSingleton<IOrganizationRepository>(new MockOrganizationRepository());
        services.AddSingleton<TimeProvider>(new MockTimeProvider());
        services.AddLogging();

        var listenerConfig = new MockListenerConfig { RoutingKey = "test-key", IntegrationType = "Test" };
        services.AddRabbitMqIntegration<object, MockListenerConfig>(listenerConfig);

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();

        // Assert
        Assert.Contains(hostedServices, s => s.GetType().Name.Contains("RabbitMqIntegrationListenerService"));
    }
}
