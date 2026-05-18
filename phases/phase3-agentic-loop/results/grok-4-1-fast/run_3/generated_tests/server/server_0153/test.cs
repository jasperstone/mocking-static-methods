using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using System;
using System.Linq;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRabbitMqEventListener_ThrowsInvalidOperationException_WhenTimeProviderNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(c => c.IntegrationType).Returns(IntegrationType.Webhook);
        mockListenerConfig.Setup(c => c.RoutingKey).Returns("test.key");

        RegisterCommonDependencies(services, mockListenerConfig.Object);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            services.AddRabbitMqEventListener(mockListenerConfig.Object));
        
        Assert.Contains("TimeProvider", exception.Message);
    }

    [Fact]
    public void AddRabbitMqEventListener_Succeeds_WhenAllDependenciesRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(c => c.IntegrationType).Returns(IntegrationType.Webhook);
        mockListenerConfig.Setup(c => c.RoutingKey).Returns("test.key");

        RegisterCommonDependencies(services, mockListenerConfig.Object);
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        // Act
        var result = services.AddRabbitMqEventListener(mockListenerConfig.Object);

        // Assert
        Assert.Same(services, result);
        var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
        Assert.NotEmpty(hostedServices);
    }

    [Fact]
    public void AddRabbitMqEventListener_RegistersServicesWithTimeProviderDependency()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(c => c.IntegrationType).Returns(IntegrationType.Webhook);
        mockListenerConfig.Setup(c => c.RoutingKey).Returns("test.key");

        RegisterCommonDependencies(services, mockListenerConfig.Object);
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        // Act - This exercises the GetRequiredService<TimeProvider>() call on line 1041
        services.AddRabbitMqEventListener(mockListenerConfig.Object);
        using var provider = services.BuildServiceProvider();

        // Assert - Successful resolution confirms GetRequiredService<TimeProvider>() worked
        var hostedServices = provider.GetServices<IHostedService>();
        Assert.NotEmpty(hostedServices);
    }

    private static void RegisterCommonDependencies(ServiceCollection services, IIntegrationListenerConfiguration listenerConfig)
    {
        services.AddSingleton(listenerConfig);
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IRabbitMqService>(Mock.Of<IRabbitMqService>());
        services.AddLogging();
    }
}
