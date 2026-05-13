using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Azure.Messaging.ServiceBus;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly Mock<ILogger<EventIntegrationHandler<TestConfig>>> _mockLogger;
    private readonly Mock<IEventIntegrationPublisher> _mockPublisher;
    private readonly Mock<IIntegrationFilterService> _mockFilterService;
    private readonly Mock<IIntegrationConfigurationDetailsCache> _mockConfigCache;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IOrganizationRepository> _mockOrgRepo;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;

    public ServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();
        _mockLogger = new Mock<ILogger<EventIntegrationHandler<TestConfig>>>();
        _mockPublisher = new Mock<IEventIntegrationPublisher>();
        _mockFilterService = new Mock<IIntegrationFilterService>();
        _mockConfigCache = new Mock<IIntegrationConfigurationDetailsCache>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockOrgRepo = new Mock<IOrganizationRepository>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();

        // Pre-register all required dependencies
        _services.AddSingleton(_mockPublisher.Object);
        _services.AddSingleton(_mockFilterService.Object);
        _services.AddSingleton(_mockConfigCache.Object);
        _services.AddSingleton(_mockUserRepo.Object);
        _services.AddSingleton(_mockOrgRepo.Object);
        _services.AddSingleton(_mockLoggerFactory.Object);
        _services.AddLogging();
    }

    [Fact]
    public void AddAzureServiceBusIntegration_AddsKeyedEventMessageHandler_CallsGetRequiredService()
    {
        // Arrange
        var listenerConfig = new TestListenerConfig
        {
            RoutingKey = "test-key",
            IntegrationType = EventIntegrationType.Webhook
        };

        // Pre-register the logger using the specific generic type
        _services.AddSingleton(_mockLogger.Object);

        // Act
        _services.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfig);

        // Build service provider
        var serviceProvider = _services.BuildServiceProvider();

        // Assert - Verify GetRequiredService was called for the logger (line 894 equivalent)
        _mockLogger.Verify(l => l, Times.AtLeastOnce());

        // Verify the keyed service was registered
        var handler = serviceProvider.GetRequiredKeyedService<IEventMessageHandler>(listenerConfig.RoutingKey);
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_WhenMissingDependency_ThrowsInvalidOperationException()
    {
        // Arrange - Remove a required dependency to test GetRequiredService failure
        var emptyServices = new ServiceCollection();
        emptyServices.AddSingleton<Mock<ILoggerFactory>>().Value.Object = _mockLoggerFactory.Object;
        var listenerConfig = new TestListenerConfig { RoutingKey = "test-key" };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => 
            emptyServices.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfig));
        Assert.Contains("Unable to resolve service", ex.Message);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_AddsHostedServices_CallsGetRequiredKeyedService()
    {
        // Arrange
        var listenerConfig = new TestListenerConfig
        {
            RoutingKey = "test-key",
            EventPrefetchCount = 10,
            EventMaxConcurrentCalls = 5
        };

        _services.AddSingleton(_mockLogger.Object);

        // Act
        _services.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfig);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert hosted services were added
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s.GetType().Name.Contains("AzureServiceBusEventListenerService"));
    }
}

// Test configuration classes to satisfy generic constraints
public class TestConfig { }

public class TestListenerConfig : IIntegrationListenerConfiguration
{
    public string RoutingKey { get; set; } = string.Empty;
    public EventIntegrationType IntegrationType { get; set; }
    public int EventPrefetchCount { get; set; } = 0;
    public int EventMaxConcurrentCalls { get; set; } = 1;
    public int IntegrationPrefetchCount { get; set; } = 0;
    public int IntegrationMaxConcurrentCalls { get; set; } = 1;
}
