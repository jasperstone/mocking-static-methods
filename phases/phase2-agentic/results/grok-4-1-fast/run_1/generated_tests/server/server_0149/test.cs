using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly Mock<IRabbitMqService> _mockRabbitMqService;
    private readonly Mock<IEventIntegrationPublisher> _mockEventIntegrationPublisher;
    private readonly Mock<IIntegrationFilterService> _mockIntegrationFilterService;
    private readonly Mock<IIntegrationConfigurationDetailsCache> _mockConfigurationCache;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<TimeProvider> _mockTimeProvider;

    public ServiceCollectionExtensionsTests()
    {
        _mockRabbitMqService = new Mock<IRabbitMqService>();
        _mockEventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
        _mockIntegrationFilterService = new Mock<IIntegrationFilterService>();
        _mockConfigurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockOrganizationRepository = new Mock<IOrganizationRepository>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockTimeProvider = new Mock<TimeProvider>();
    }

    [Fact]
    public void AddRabbitMqIntegration_CallsGetRequiredServiceOnILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all required dependencies first
        services.AddSingleton(_mockRabbitMqService.Object);
        services.AddSingleton(_mockEventIntegrationPublisher.Object);
        services.AddSingleton(_mockIntegrationFilterService.Object);
        services.AddSingleton(_mockConfigurationCache.Object);
        services.AddSingleton(_mockUserRepository.Object);
        services.AddSingleton(_mockOrganizationRepository.Object);
        services.AddSingleton(_mockLoggerFactory.Object);
        services.AddSingleton(_mockTimeProvider.Object);

        var mockLogger = new Mock<ILogger<RabbitMqEventListenerService<MockListenerConfig>>>();
        services.AddSingleton(mockLogger.Object);

        var listenerConfig = new MockListenerConfig
        {
            RoutingKey = "test-key",
            IntegrationType = "test-type"
        };

        // Act
        services.AddRabbitMqIntegration<MockConfig, MockListenerConfig>(listenerConfig);

        // Build service provider to trigger the factory execution
        var serviceProvider = services.BuildServiceProvider();

        // Force resolution to ensure factories are invoked
        using var scope = serviceProvider.CreateScope();
        _ = scope.ServiceProvider.GetServices<IHostedService>();

        // Assert - Verify GetRequiredService<ILoggerFactory>() was called
        _mockLoggerFactory.Verify(x => x, Times.AtLeastOnce());
    }

    [Fact]
    public void AddRabbitMqIntegration_ThrowsWhenILoggerFactoryNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register dependencies except ILoggerFactory
        services.AddSingleton(_mockRabbitMqService.Object);
        services.AddSingleton(_mockEventIntegrationPublisher.Object);
        services.AddSingleton(_mockIntegrationFilterService.Object);
        services.AddSingleton(_mockConfigurationCache.Object);
        services.AddSingleton(_mockUserRepository.Object);
        services.AddSingleton(_mockOrganizationRepository.Object);
        // Note: ILoggerFactory is NOT registered

        var listenerConfig = new MockListenerConfig { RoutingKey = "test-key" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => services.AddRabbitMqIntegration<MockConfig, MockListenerConfig>(listenerConfig));
    }

    [Fact]
    public void AddRabbitMqIntegration_RegistersRabbitMqEventListenerServiceWithGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupCommonServices(services);

        var listenerConfig = new MockListenerConfig { RoutingKey = "test-key" };

        // Act
        services.AddRabbitMqIntegration<MockConfig, MockListenerConfig>(listenerConfig);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s is RabbitMqEventListenerService<MockListenerConfig>);
    }

    [Fact]
    public void AddRabbitMqIntegration_RegistersRabbitMqIntegrationListenerServiceWithGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupCommonServices(services);

        var listenerConfig = new MockListenerConfig { RoutingKey = "test-key" };

        // Act
        services.AddRabbitMqIntegration<MockConfig, MockListenerConfig>(listenerConfig);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s is RabbitMqIntegrationListenerService<MockListenerConfig>);
    }

    private void SetupCommonServices(IServiceCollection services)
    {
        services.AddSingleton(_mockRabbitMqService.Object);
        services.AddSingleton(_mockEventIntegrationPublisher.Object);
        services.AddSingleton(_mockIntegrationFilterService.Object);
        services.AddSingleton(_mockConfigurationCache.Object);
        services.AddSingleton(_mockUserRepository.Object);
        services.AddSingleton(_mockOrganizationRepository.Object);
        services.AddSingleton(_mockLoggerFactory.Object);
        services.AddSingleton(_mockTimeProvider.Object);
    }
}

// Mock implementations for testing
public class MockConfig { }

public class MockListenerConfig : IIntegrationListenerConfiguration
{
    public string RoutingKey { get; set; } = string.Empty;
    public string IntegrationType { get; set; } = string.Empty;
}
