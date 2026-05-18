using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.HostedServices;
using Bit.Core.Repositories;
using Bit.Core.Services;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRabbitMqEventListener_ExecutesSuccessfullyWithAllDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all required dependencies
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddSingleton<IRabbitMqService>(Mock.Of<IRabbitMqService>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        services.AddSingleton<TimeProvider>(Mock.Of<TimeProvider>());
        
        // Add logging for the generic logger
        services.AddLogging();

        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(x => x.RoutingKey).Returns("test-key");
        mockListenerConfig.Setup(x => x.IntegrationType).Returns("test-type");

        // Act
        var result = Bit.SharedWeb.Utilities.ServiceCollectionExtensions
            .AddRabbitMqEventListener(services, mockListenerConfig.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);

        // Verify the extension executed without throwing (covers all GetRequiredService calls including TimeProvider)
        using var provider = result.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>();
        Assert.NotEmpty(hostedServices);
    }

    [Fact]
    public void AddRabbitMqEventListener_MissingTimeProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddSingleton<IRabbitMqService>(Mock.Of<IRabbitMqService>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        // Missing TimeProvider intentionally

        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(x => x.RoutingKey).Returns("test-key");
        mockListenerConfig.Setup(x => x.IntegrationType).Returns("test-type");

        // Act & Assert - specifically tests the GetRequiredService<TimeProvider>() call on line ~1041
        var ex = Assert.Throws<InvalidOperationException>(() => 
            Bit.SharedWeb.Utilities.ServiceCollectionExtensions
                .AddRabbitMqEventListener(services, mockListenerConfig.Object));
        
        Assert.Contains("TimeProvider", ex.Message);
    }
}
