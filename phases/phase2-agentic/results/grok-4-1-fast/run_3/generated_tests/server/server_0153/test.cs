using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Bit.Core;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.HostedServices;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly Mock<ILogger<EventIntegrationHandler<object>>> _mockEventLogger;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<IEventIntegrationPublisher> _mockEventPublisher;
    private readonly Mock<IIntegrationFilterService> _mockFilterService;
    private readonly Mock<IIntegrationConfigurationDetailsCache> _mockConfigCache;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IOrganizationRepository> _mockOrgRepo;
    private readonly Mock<IRabbitMqService> _mockRabbitMqService;
    private readonly Mock<TimeProvider> _mockTimeProvider;

    public ServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();
        _mockEventLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockEventPublisher = new Mock<IEventIntegrationPublisher>();
        _mockFilterService = new Mock<IIntegrationFilterService>();
        _mockConfigCache = new Mock<IIntegrationConfigurationDetailsCache>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockOrgRepo = new Mock<IOrganizationRepository>();
        _mockRabbitMqService = new Mock<IRabbitMqService>();
        _mockTimeProvider = new Mock<TimeProvider>();
    }

    [Fact]
    public void AddRabbitMqEventListener_ThrowsWhenTimeProviderNotRegistered()
    {
        // Arrange
        var listenerConfig = new RabbitMqEventListenerConfiguration
        {
            RoutingKey = "test-key",
            IntegrationType = "test-type"
        };

        SetupCoreServices();

        // ACT & ASSERT
        Assert.Throws<InvalidOperationException>(() => _services.AddRabbitMqEventListener(listenerConfig));
    }

    [Fact]
    public void AddRabbitMqEventListener_SucceedsWithAllRequiredServicesIncludingTimeProvider()
    {
        // Arrange
        var listenerConfig = new RabbitMqEventListenerConfiguration
        {
            RoutingKey = "test-key",
            IntegrationType = "test-type"
        };

        SetupCoreServices();
        _services.AddSingleton(_mockTimeProvider.Object);

        // Act
        _services.AddRabbitMqEventListener(listenerConfig);

        // Assert - Build service provider and verify GetRequiredService was called for TimeProvider
        var serviceProvider = _services.BuildServiceProvider();
        
        // Verify the extension successfully registered services without throwing
        Assert.NotNull(serviceProvider);
        
        // Verify we can resolve the hosted service which uses GetRequiredService<TimeProvider>
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s.GetType().Name == "RabbitMqIntegrationListenerService`1");
    }

    [Fact]
    public void AddRabbitMqEventListener_VerifiesGetRequiredServiceForTimeProviderInListenerService()
    {
        // Arrange
        var listenerConfig = new RabbitMqEventListenerConfiguration
        {
            RoutingKey = "test-key",
            IntegrationType = "test-type"
        };

        SetupCoreServices();
        _services.AddSingleton(_mockTimeProvider.Object);

        // Act
        _services.AddRabbitMqEventListener(listenerConfig);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert - The RabbitMqIntegrationListenerService constructor calls provider.GetRequiredService<TimeProvider>()
        // This test verifies the registration succeeds and the service can be resolved
        var listenerServices = serviceProvider.GetServices<IHostedService>();
        var targetService = Assert.Single(listenerServices, s => 
            s.GetType().Name.StartsWith("RabbitMqIntegrationListenerService"));

        // Verify the service was created successfully (implying GetRequiredService<TimeProvider>() succeeded)
        Assert.NotNull(targetService);
    }

    private void SetupCoreServices()
    {
        _services.AddSingleton(_mockEventPublisher.Object);
        _services.AddSingleton(_mockFilterService.Object);
        _services.AddSingleton(_mockConfigCache.Object);
        _services.AddSingleton(_mockUserRepo.Object);
        _services.AddSingleton(_mockOrgRepo.Object);
        _services.AddSingleton(_mockLoggerFactory.Object);
        _services.AddSingleton(_mockEventLogger.Object);
        _services.AddSingleton<IRabbitMqService>(_mockRabbitMqService.Object);
    }
}
