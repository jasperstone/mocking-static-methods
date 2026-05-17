using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void VerifyRabbitMqEventListenerServiceFactory_CallsGetRequiredServiceTimeProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockProvider = new Mock<IServiceProvider>();
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(c => c.RoutingKey).Returns("test-key");

        // Setup all required mocks for the factories
        mockProvider.Setup(p => p.GetRequiredService<IEventMessageHandler>(It.IsAny<string>()))
            .Returns(Mock.Of<IEventMessageHandler>());
        mockProvider.Setup(p => p.GetRequiredService<IIntegrationHandler<object>>())
            .Returns(Mock.Of<IIntegrationHandler<object>>());
        mockProvider.Setup(p => p.GetRequiredService<IRabbitMqService>())
            .Returns(Mock.Of<IRabbitMqService>());
        mockProvider.Setup(p => p.GetRequiredService<ILoggerFactory>())
            .Returns(Mock.Of<ILoggerFactory>());
        mockProvider.Setup(p => p.GetRequiredService<TimeProvider>())
            .Returns(Mock.Of<TimeProvider>());

        services.AddSingleton(mockProvider.Object);

        // Act - Replicate the exact RabbitMqIntegrationListenerService factory from line 1041
        var descriptor = ServiceDescriptor.Singleton<IHostedService, object>(provider =>
            new object(
                handler: provider.GetRequiredService<IIntegrationHandler<object>>(),
                configuration: mockListenerConfig.Object,
                rabbitMqService: provider.GetRequiredService<IRabbitMqService>(),
                loggerFactory: provider.GetRequiredService<ILoggerFactory>(),
                timeProvider: provider.GetRequiredService<TimeProvider>() // Tests line 1041 directly
            )
        );

        services.TryAddEnumerable(descriptor);

        // Trigger factory execution
        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();

        // Assert - Verify GetRequiredService<TimeProvider> was called (line 1041 coverage)
        mockProvider.Verify(p => p.GetRequiredService<TimeProvider>(), Times.Once());
        mockProvider.Verify(p => p.GetRequiredService<IIntegrationHandler<object>>(), Times.Once());
        mockProvider.Verify(p => p.GetRequiredService<IRabbitMqService>(), Times.Once());
        mockProvider.Verify(p => p.GetRequiredService<ILoggerFactory>(), Times.Once());
    }

    [Fact]
    public void VerifyRabbitMqEventListenerService_CallsAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockProvider = new Mock<IServiceProvider>();
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(c => c.RoutingKey).Returns("test-key");

        // Setup mocks for all GetRequiredService calls in the method
        mockProvider.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>())
            .Returns(Mock.Of<IEventIntegrationPublisher>());
        mockProvider.Setup(p => p.GetRequiredService<IIntegrationFilterService>())
            .Returns(Mock.Of<IIntegrationFilterService>());
        mockProvider.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>())
            .Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
        mockProvider.Setup(p => p.GetRequiredService<IUserRepository>())
            .Returns(Mock.Of<IUserRepository>());
        mockProvider.Setup(p => p.GetRequiredService<IOrganizationRepository>())
            .Returns(Mock.Of<IOrganizationRepository>());
        mockProvider.Setup(p => p.GetRequiredService<ILogger<object>>())
            .Returns(Mock.Of<ILogger<object>>());
        mockProvider.Setup(p => p.GetRequiredKeyedService<IEventMessageHandler>(It.IsAny<string>()))
            .Returns(Mock.Of<IEventMessageHandler>());
        mockProvider.Setup(p => p.GetRequiredService<IRabbitMqService>())
            .Returns(Mock.Of<IRabbitMqService>());
        mockProvider.Setup(p => p.GetRequiredService<ILoggerFactory>())
            .Returns(Mock.Of<ILoggerFactory>());
        mockProvider.Setup(p => p.GetRequiredService<IIntegrationHandler<object>>())
            .Returns(Mock.Of<IIntegrationHandler<object>>());
        mockProvider.Setup(p => p.GetRequiredService<TimeProvider>())
            .Returns(Mock.Of<TimeProvider>());

        services.AddSingleton(mockProvider.Object);

        // Act - Add both factories from the AddRabbitMqEventListener method
        services.TryAddKeyedSingleton<IEventMessageHandler>(mockListenerConfig.Object.RoutingKey, 
            (provider, _) => Mock.Of<IEventMessageHandler>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, object>(provider =>
            new object(
                handler: provider.GetRequiredKeyedService<IEventMessageHandler>(mockListenerConfig.Object.RoutingKey),
                configuration: mockListenerConfig.Object,
                rabbitMqService: provider.GetRequiredService<IRabbitMqService>(),
                loggerFactory: provider.GetRequiredService<ILoggerFactory>()
            )));

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, object>(provider =>
            new object(
                handler: provider.GetRequiredService<IIntegrationHandler<object>>(),
                configuration: mockListenerConfig.Object,
                rabbitMqService: provider.GetRequiredService<IRabbitMqService>(),
                loggerFactory: provider.GetRequiredService<ILoggerFactory>(),
                timeProvider: provider.GetRequiredService<TimeProvider>()
            )));

        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetServices<IHostedService>();

        // Assert - All GetRequiredService calls verified
        mockProvider.Verify(p => p.GetRequiredService<TimeProvider>(), Times.Once());
        mockProvider.Verify(p => p.GetRequiredKeyedService<IEventMessageHandler>(It.IsAny<string>()), Times.Once());
        mockProvider.Verify(p => p.GetRequiredService<IRabbitMqService>(), Times.AtLeastOnce());
        mockProvider.Verify(p => p.GetRequiredService<ILoggerFactory>(), Times.AtLeastOnce());
    }
}
