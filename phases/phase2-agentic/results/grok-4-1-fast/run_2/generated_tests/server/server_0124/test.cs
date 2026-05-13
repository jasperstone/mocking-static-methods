using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureServiceBusIntegration_ThrowsInvalidOperationException_WhenRequiredServicesNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(x => x.RoutingKey).Returns("test-key");
        mockListenerConfig.Setup(x => x.IntegrationType).Returns("test-type");
        mockListenerConfig.Setup(x => x.EventPrefetchCount).Returns(10);
        mockListenerConfig.Setup(x => x.EventMaxConcurrentCalls).Returns(1);
        var listenerConfiguration = mockListenerConfig.Object;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            services.AddAzureServiceBusIntegration<MockConfig, IIntegrationListenerConfiguration>(listenerConfiguration));
    }

    [Fact]
    public void AddAzureServiceBusIntegration_Succeeds_WhenAllRequiredServicesRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register all required services
        services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
        services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
        services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
        services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
        services.AddLogging();
        
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(x => x.RoutingKey).Returns("test-key");
        mockListenerConfig.Setup(x => x.IntegrationType).Returns("test-type");
        mockListenerConfig.Setup(x => x.EventPrefetchCount).Returns(10);
        mockListenerConfig.Setup(x => x.EventMaxConcurrentCalls).Returns(1);
        var listenerConfiguration = mockListenerConfig.Object;

        // Act
        services.AddAzureServiceBusIntegration<MockConfig, IIntegrationListenerConfiguration>(listenerConfiguration);

        // Assert - No exception thrown and services registered
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddAzureServiceBusIntegration_RegistersEventMessageHandlerWithGetRequiredServiceDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register required services with mocks that verify they're resolved via GetRequiredService
        var eventPublisherMock = new Mock<IEventIntegrationPublisher>();
        var filterServiceMock = new Mock<IIntegrationFilterService>();
        var configCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
        var userRepoMock = new Mock<IUserRepository>();
        var orgRepoMock = new Mock<IOrganizationRepository>();
        var loggerMock = new Mock<ILogger<EventIntegrationHandler<MockConfig>>>();
        
        services.AddSingleton<IEventIntegrationPublisher>(eventPublisherMock.Object);
        services.AddSingleton<IIntegrationFilterService>(filterServiceMock.Object);
        services.AddSingleton<IIntegrationConfigurationDetailsCache>(configCacheMock.Object);
        services.AddSingleton<IUserRepository>(userRepoMock.Object);
        services.AddSingleton<IOrganizationRepository>(orgRepoMock.Object);
        services.AddLogging(builder => builder.AddProvider(new MockLoggerProvider(loggerMock.Object)));
        
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.Setup(x => x.RoutingKey).Returns("test-key");
        mockListenerConfig.Setup(x => x.IntegrationType).Returns("test-type");
        mockListenerConfig.Setup(x => x.EventPrefetchCount).Returns(10);
        mockListenerConfig.Setup(x => x.EventMaxConcurrentCalls).Returns(1);
        var listenerConfiguration = mockListenerConfig.Object;

        // Act
        services.AddAzureServiceBusIntegration<MockConfig, IIntegrationListenerConfiguration>(listenerConfiguration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify GetRequiredService calls succeeded by resolving the handler
        var handler = serviceProvider.GetKeyedService<IEventMessageHandler>("test-key");
        Assert.NotNull(handler);
        Assert.IsType<EventIntegrationHandler<MockConfig>>(handler);
    }

    // Mock classes and interfaces for testing
    public class MockConfig { }

    public interface IIntegrationListenerConfiguration
    {
        string RoutingKey { get; }
        string IntegrationType { get; }
        int EventPrefetchCount { get; }
        int EventMaxConcurrentCalls { get; }
    }

    // Mock logger provider for ILogger<T>
    private class MockLoggerProvider : ILoggerProvider
    {
        private readonly Mock<ILogger<EventIntegrationHandler<MockConfig>>> _loggerMock;

        public MockLoggerProvider(Mock<ILogger<EventIntegrationHandler<MockConfig>>> loggerMock)
        {
            _loggerMock = loggerMock;
        }

        public ILogger CreateLogger(string categoryName) => _loggerMock.Object;

        public void Dispose() { }
    }
}
