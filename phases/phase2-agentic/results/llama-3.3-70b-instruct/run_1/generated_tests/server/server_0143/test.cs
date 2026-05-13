using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRabbitMqIntegration_ResolvesServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
        var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
        var integrationFilterService = new Mock<IIntegrationFilterService>();
        var integrationConfigurationDetailsCache = new Mock<IIntegrationConfigurationDetailsCache>();
        var userRepository = new Mock<IUserRepository>();
        var organizationRepository = new Mock<IOrganizationRepository>();
        var logger = new Mock<ILogger<EventIntegrationHandler<MockIntegrationConfigurationDetails>>>();

        services.AddSingleton(listenerConfiguration.Object);
        services.AddSingleton(eventIntegrationPublisher.Object);
        services.AddSingleton(integrationFilterService.Object);
        services.AddSingleton(integrationConfigurationDetailsCache.Object);
        services.AddSingleton(userRepository.Object);
        services.AddSingleton(organizationRepository.Object);
        services.AddSingleton(logger.Object);

        // Act
        services.AddRabbitMqIntegration<MockIntegrationConfigurationDetails, MockIntegrationListenerConfiguration>(listenerConfiguration.Object);

        // Assert
        var serviceProviderAfterAdd = services.BuildServiceProvider();
        var eventIntegrationHandler = serviceProviderAfterAdd.GetRequiredService<IEventMessageHandler>();
        var rabbitMqEventListenerService = serviceProviderAfterAdd.GetRequiredService<RabbitMqEventListenerService<MockIntegrationListenerConfiguration>>();
        var rabbitMqIntegrationListenerService = serviceProviderAfterAdd.GetRequiredService<RabbitMqIntegrationListenerService<MockIntegrationListenerConfiguration>>();

        Assert.NotNull(eventIntegrationHandler);
        Assert.NotNull(rabbitMqEventListenerService);
        Assert.NotNull(rabbitMqIntegrationListenerService);
    }

    [Fact]
    public void GetRequiredService_ResolvesIIntegrationFilterServiceCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var integrationFilterService = new Mock<IIntegrationFilterService>();

        services.AddSingleton(integrationFilterService.Object);

        // Act
        var resolvedService = serviceProvider.GetRequiredService<IIntegrationFilterService>();

        // Assert
        Assert.NotNull(resolvedService);
        Assert.Equal(integrationFilterService.Object, resolvedService);
    }
}

public class MockIntegrationConfigurationDetails { }

public class MockIntegrationListenerConfiguration : IIntegrationListenerConfiguration
{
    public string RoutingKey => string.Empty;
    public IntegrationType IntegrationType => IntegrationType.None;
}
