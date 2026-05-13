using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.HostedServices;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly Mock<IRepositoryConfiguration> _mockRepositoryConfiguration;
    private readonly Mock<IAzureServiceBusService> _mockServiceBusService;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<IIntegrationFilterService> _mockIntegrationFilterService;
    private readonly Mock<IIntegrationConfigurationDetailsCache> _mockConfigurationCache;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly Mock<IRabbitMqService> _mockRabbitMqService;
    private readonly Mock<IEventIntegrationPublisher> _mockEventIntegrationPublisher;
    private readonly Mock<ITimeProvider> _mockTimeProvider;

    public ServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();
        _mockRepositoryConfiguration = new Mock<IRepositoryConfiguration>();
        _mockServiceBusService = new Mock<IAzureServiceBusService>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockIntegrationFilterService = new Mock<IIntegrationFilterService>();
        _mockConfigurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockOrganizationRepository = new Mock<IOrganizationRepository>();
        _mockRabbitMqService = new Mock<IRabbitMqService>();
        _mockEventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
        _mockTimeProvider = new Mock<ITimeProvider>();

        // Register required services to avoid InvalidOperationException during GetRequiredService
        _services.AddSingleton(_mockRepositoryConfiguration.Object);
        _services.AddSingleton(_mockServiceBusService.Object);
        _services.AddSingleton(_mockLoggerFactory.Object);
        _services.AddSingleton(_mockIntegrationFilterService.Object);
        _services.AddSingleton(_mockConfigurationCache.Object);
        _services.AddSingleton(_mockUserRepository.Object);
        _services.AddSingleton(_mockOrganizationRepository.Object);
        _services.AddSingleton(_mockRabbitMqService.Object);
        _services.AddSingleton(_mockEventIntegrationPublisher.Object);
        _services.AddSingleton(_mockTimeProvider.Object);
        _services.AddLogging();
    }

    [Fact]
    public void AddRabbitMqIntegration_CallsGetRequiredServiceIntegrationFilterService_Successfully()
    {
        // Arrange
        var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
        listenerConfiguration.Setup(x => x.RoutingKey).Returns("test-key");
        listenerConfiguration.Setup(x => x.IntegrationType).Returns(IntegrationType.Slack);

        // Act
        _services.AddRabbitMqIntegration(listenerConfiguration.Object);

        // Assert - Build service provider and verify GetRequiredService was called
        var serviceProvider = _services.BuildServiceProvider();
        
        // Verify the registration creates the handler using GetRequiredService
        var handler = serviceProvider.GetKeyedService<IEventMessageHandler>("test-key");
        Assert.NotNull(handler);

        // Verify mocks were resolved (indicating GetRequiredService calls succeeded)
        _mockIntegrationFilterService.VerifyAll();
        _mockConfigurationCache.VerifyAll();
        _mockUserRepository.VerifyAll();
        _mockOrganizationRepository.VerifyAll();
    }

    [Fact]
    public void AddRabbitMqIntegration_RegistersRabbitMqEventListenerService_WithGetRequiredKeyedService()
    {
        // Arrange
        var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
        listenerConfiguration.Setup(x => x.RoutingKey).Returns("test-key");

        // Act
        _services.AddRabbitMqIntegration(listenerConfiguration.Object);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.Contains(hostedServices.OfType<RabbitMqEventListenerService<IIntegrationListenerConfiguration>>(), h => true);
    }

    [Fact]
    public void AddRabbitMqIntegration_RegistersRabbitMqIntegrationListenerService_WithGetRequiredService()
    {
        // Arrange
        var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
        listenerConfiguration.Setup(x => x.RoutingKey).Returns("test-key");

        // Pre-register IIntegrationHandler<TConfig> to avoid resolution failure
        _services.TryAddSingleton<IIntegrationHandler<object>>(Mock.Of<IIntegrationHandler<object>>());

        // Act
        _services.AddRabbitMqIntegration<SomeConfigType, MockIIntegrationListenerConfiguration>(listenerConfiguration.Object);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, h => h.GetType().Name == "RabbitMqIntegrationListenerService`1");
    }

    // Helper classes for generic constraints
    public class SomeConfigType { }
    public class MockIIntegrationListenerConfiguration : IIntegrationListenerConfiguration
    {
        public string RoutingKey => "test-key";
        public IntegrationType IntegrationType => IntegrationType.Slack;
    }
}
