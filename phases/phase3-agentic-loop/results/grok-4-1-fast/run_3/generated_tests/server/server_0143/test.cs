using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    private class MockIntegrationListenerConfiguration : IIntegrationListenerConfiguration
    {
        public string RoutingKey => "test-key";
        public string IntegrationType => "TestType";
    }

    [Fact]
    public void AddRabbitMqIntegration_SuccessfullyRegistersServices_WithAllDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        var listenerConfig = new MockIntegrationListenerConfiguration();

        // Add all required dependencies
        services.AddSingleton(Mock.Of<Bit.Core.Services.IEventIntegrationPublisher>());
        services.AddSingleton(Mock.Of<Bit.Core.Services.IIntegrationFilterService>());
        services.AddSingleton(Mock.Of<Bit.Core.Services.IIntegrationConfigurationDetailsCache>());
        services.AddSingleton(Mock.Of<Bit.Core.Repositories.IUserRepository>());
        services.AddSingleton(Mock.Of<Bit.Core.Repositories.IOrganizationRepository>());
        services.AddSingleton(Mock.Of<Bit.Core.Services.IRabbitMqService>());
        services.AddSingleton(Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());
        services.AddSingleton<Microsoft.Extensions.TimeProvider.SystemTimeProvider>();

        // Act
        services.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(listenerConfig);

        // Assert - Build provider and resolve to trigger GetRequiredService calls
        using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredKeyedService<Bit.Core.HostedServices.IEventMessageHandler>("test-key");
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddRabbitMqIntegration_MissingIntegrationFilterService_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var listenerConfig = new MockIntegrationListenerConfiguration();

        // Add all dependencies EXCEPT IIntegrationFilterService (line 1017)
        services.AddSingleton(Mock.Of<Bit.Core.Services.IEventIntegrationPublisher>());
        services.AddSingleton(Mock.Of<Bit.Core.Services.IIntegrationConfigurationDetailsCache>());
        services.AddSingleton(Mock.Of<Bit.Core.Repositories.IUserRepository>());
        services.AddSingleton(Mock.Of<Bit.Core.Repositories.IOrganizationRepository>());
        services.AddSingleton(Mock.Of<Bit.Core.Services.IRabbitMqService>());
        services.AddSingleton(Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());
        services.AddSingleton<Microsoft.Extensions.TimeProvider.SystemTimeProvider>();

        // Act
        services.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(listenerConfig);
        using var provider = services.BuildServiceProvider();

        // Assert - GetRequiredService<IIntegrationFilterService> should throw
        Assert.Throws<InvalidOperationException>(() => 
            provider.GetRequiredKeyedService<Bit.Core.HostedServices.IEventMessageHandler>("test-key"));
    }

    [Fact]
    public void AddRabbitMqIntegration_MissingUserRepository_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var listenerConfig = new MockIntegrationListenerConfiguration();

        // Add all dependencies EXCEPT IUserRepository
        services.AddSingleton(Mock.Of<Bit.Core.Services.IEventIntegrationPublisher>());
        services.AddSingleton(Mock.Of<Bit.Core.Services.IIntegrationFilterService>());
        services.AddSingleton(Mock.Of<Bit.Core.Services.IIntegrationConfigurationDetailsCache>());
        services.AddSingleton(Mock.Of<Bit.Core.Repositories.IOrganizationRepository>());
        services.AddSingleton(Mock.Of<Bit.Core.Services.IRabbitMqService>());
        services.AddSingleton(Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());
        services.AddSingleton<Microsoft.Extensions.TimeProvider.SystemTimeProvider>();

        // Act
        services.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(listenerConfig);
        using var provider = services.BuildServiceProvider();

        // Assert - GetRequiredService<IUserRepository> should throw
        Assert.Throws<InvalidOperationException>(() => 
            provider.GetRequiredKeyedService<Bit.Core.HostedServices.IEventMessageHandler>("test-key"));
    }
}
