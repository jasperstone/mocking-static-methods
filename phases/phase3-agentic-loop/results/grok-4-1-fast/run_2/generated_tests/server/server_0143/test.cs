using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.HostedServices;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly Mock<IIntegrationListenerConfiguration> _mockListenerConfig;

    public ServiceCollectionExtensionsTests()
    {
        _mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        _mockListenerConfig.SetupGet(x => x.RoutingKey).Returns("test-key");
        _mockListenerConfig.SetupGet(x => x.IntegrationType).Returns("TestType");
    }

    [Fact]
    public void AddRabbitMqIntegration_WithAllDependencies_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all dependencies required by the factory at line 1017
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddLogging();

        // Act - this exercises provider.GetRequiredService<IIntegrationFilterService>() at line 1017
        services.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(_mockListenerConfig.Object);
        var provider = services.BuildServiceProvider();
        
        // Trigger factory execution
        _ = provider.GetRequiredKeyedService<IEventMessageHandler>("test-key");

        // Assert
        Assert.True(true); // Reached here without exception
    }

    [Fact]
    public void AddRabbitMqIntegration_MissingIIntegrationFilterService_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Missing IIntegrationFilterService - this will fail at line 1017
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddLogging();

        // Act & Assert
        services.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(_mockListenerConfig.Object);
        var provider = services.BuildServiceProvider();
        
        var ex = Assert.Throws<InvalidOperationException>(() => 
            provider.GetRequiredKeyedService<IEventMessageHandler>("test-key"));
        Assert.Contains("Unable to resolve service for type", ex.Message);
    }

    [Fact]
    public void AddRabbitMqIntegration_ExercisesGetRequiredKeyedServiceInHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register keyed service first to avoid factory issues
        services.AddSingleton<IEventMessageHandler>("test-key", Mock.Of<IEventMessageHandler>());
        services.AddSingleton<IRabbitMqService>(Mock.Of<IRabbitMqService>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        services.AddSingleton<TimeProvider>(Mock.Of<TimeProvider>());

        // Act - exercises GetRequiredKeyedService calls in hosted service factories
        services.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(_mockListenerConfig.Object);
        var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>();

        // Assert
        Assert.True(hostedServices.Any());
    }
}
