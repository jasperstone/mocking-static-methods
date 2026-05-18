using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureServiceBusIntegration_ExecutesWithoutException_WhenAllDependenciesRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all dependencies that GetRequiredService expects
        services.AddSingleton(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton(Mock.Of<IUserRepository>());
        services.AddSingleton(Mock.Of<IOrganizationRepository>());
        services.AddLogging(builder => builder.AddConsole());
        
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>().Object;

        // Act
        services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfig);
        using var serviceProvider = services.BuildServiceProvider();

        // Assert - GetRequiredService calls succeeded (no exception thrown)
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.NotEmpty(hostedServices);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_ThrowsInvalidOperationException_WhenEventIntegrationPublisherMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all but the first GetRequiredService dependency
        services.AddSingleton(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton(Mock.Of<IUserRepository>());
        services.AddSingleton(Mock.Of<IOrganizationRepository>());
        services.AddLogging();
        
        var listenerConfig = new Mock<IIntegrationListenerConfiguration>().Object;

        // Act & Assert
        services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfig);
        using var serviceProvider = services.BuildServiceProvider();
        
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetServices<IHostedService>());
    }
}
