using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

#nullable enable

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly Mock<ILoggerFactory> _mockLoggerFactory = new();
    private readonly Mock<IRabbitMqService> _mockRabbitMqService = new();
    private readonly Mock<IEventIntegrationPublisher> _mockEventIntegrationPublisher = new();
    private readonly Mock<IIntegrationFilterService> _mockIntegrationFilterService = new();
    private readonly Mock<IIntegrationConfigurationDetailsCache> _mockConfigurationCache = new();
    private readonly Mock<IUserRepository> _mockUserRepository = new();
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository = new();
    private readonly Mock<IIntegrationHandler<object>> _mockIntegrationHandler = new();

    public ServiceCollectionExtensionsTests()
    {
        _services.AddSingleton(_mockLoggerFactory.Object);
        _services.AddSingleton(_mockRabbitMqService.Object);
        _services.AddSingleton(_mockEventIntegrationPublisher.Object);
        _services.AddSingleton(_mockIntegrationFilterService.Object);
        _services.AddSingleton(_mockConfigurationCache.Object);
        _services.AddSingleton(_mockUserRepository.Object);
        _services.AddSingleton(_mockOrganizationRepository.Object);
        _services.AddSingleton(_mockIntegrationHandler.Object);
        _services.AddSingleton<TimeProvider>(SystemTimeProvider.Instance);
    }

    [Fact]
    public void AddRabbitMqEventListener_ThrowsWhenTimeProviderNotRegistered()
    {
        // Arrange
        var servicesWithoutTimeProvider = new ServiceCollection();
        servicesWithoutTimeProvider.AddSingleton(_mockLoggerFactory.Object);
        // ... other services but NO TimeProvider

        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(c => c.RoutingKey).Returns("test-key");
        listenerConfig.Setup(c => c.IntegrationType).Returns("test-type");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            servicesWithoutTimeProvider.AddRabbitMqEventListener(listenerConfig.Object));
        Assert.Contains("TimeProvider", ex.Message);
    }

    [Fact]
    public void AddRabbitMqEventListener_SucceedsWhenTimeProviderRegistered()
    {
        // Arrange
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(c => c.RoutingKey).Returns("test-key");
        listenerConfig.Setup(c => c.IntegrationType).Returns("test-type");

        // Act
        _services.AddRabbitMqEventListener(listenerConfig.Object);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert - No exception thrown, services registered
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.NotEmpty(hostedServices);
    }

    [Fact]
    public void AddRabbitMqEventListener_ResolvesRabbitMqIntegrationListenerServiceWithTimeProvider()
    {
        // Arrange
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(c => c.RoutingKey).Returns("test-key");
        listenerConfig.Setup(c => c.IntegrationType).Returns("test-type");

        _services.AddRabbitMqEventListener(listenerConfig.Object);
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var hostedServices = serviceProvider.GetServices<IHostedService>();

        // Assert
        var rabbitListener = Assert.Single(hostedServices.OfType<RabbitMqIntegrationListenerService<object>>());
        Assert.NotNull(rabbitListener);
        Assert.NotNull(rabbitListener.timeProvider);
    }

    [Fact]
    public void AddRabbitMqEventListener_ResolvesEventMessageHandlerWithGetRequiredServiceDependencies()
    {
        // Arrange
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
        listenerConfig.Setup(c => c.RoutingKey).Returns("test-key");
        listenerConfig.Setup(c => c.IntegrationType).Returns("test-type");

        _services.AddRabbitMqEventListener(listenerConfig.Object);
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var handler = serviceProvider.GetKeyedService<IEventMessageHandler>("test-key");

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void IsAzureServiceBusEnabled_ReturnsFalse_WhenConnectionStringMissing()
    {
        // Arrange
        var settings = new GlobalSettings
        {
            EventLogging = new()
            {
                AzureServiceBus = new()
                {
                    EventTopicName = "topic"
                    // No ConnectionString
                }
            }
        };

        // Act
        var result = ServiceCollectionExtensions.IsAzureServiceBusEnabled(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAzureServiceBusEnabled_ReturnsFalse_WhenTopicNameMissing()
    {
        // Arrange
        var settings = new GlobalSettings
        {
            EventLogging = new()
            {
                AzureServiceBus = new()
                {
                    ConnectionString = "connstr"
                    // No EventTopicName
                }
            }
        };

        // Act
        var result = ServiceCollectionExtensions.IsAzureServiceBusEnabled(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAzureServiceBusEnabled_ReturnsTrue_WhenBothSettingsPresent()
    {
        // Arrange
        var settings = new GlobalSettings
        {
            EventLogging = new()
            {
                AzureServiceBus = new()
                {
                    ConnectionString = "connstr",
                    EventTopicName = "topic"
                }
            }
        };

        // Act
        var result = ServiceCollectionExtensions.IsAzureServiceBusEnabled(settings);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRabbitMqEnabled_ReturnsFalse_WhenAnyRequiredSettingMissing()
    {
        // Arrange
        var settings = new GlobalSettings
        {
            EventLogging = new()
            {
                RabbitMq = new()
                {
                    Username = "user",
                    Password = "pass",
                    EventExchangeName = "exchange"
                    // No HostName
                }
            }
        };

        // Act
        var result = ServiceCollectionExtensions.IsRabbitMqEnabled(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRabbitMqEnabled_ReturnsTrue_WhenAllRequiredSettingsPresent()
    {
        // Arrange
        var settings = new GlobalSettings
        {
            EventLogging = new()
            {
                RabbitMq = new()
                {
                    HostName = "localhost",
                    Username = "user",
                    Password = "pass",
                    EventExchangeName = "exchange"
                }
            }
        };

        // Act
        var result = ServiceCollectionExtensions.IsRabbitMqEnabled(settings);

        // Assert
        Assert.True(result);
    }
}
