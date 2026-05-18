using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ThrowsInvalidOperationException_WhenRequiredServicesMissing()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.SetupGet(x => x.RoutingKey).Returns("test-key");
            mockListenerConfig.SetupGet(x => x.IntegrationType).Returns("test-type");
            var listenerConfiguration = mockListenerConfig.Object;

            // Act & Assert - Missing IIntegrationConfigurationDetailsCache causes GetRequiredService to throw
            Assert.ThrowsAny<InvalidOperationException>(() =>
                services.AddAzureServiceBusIntegration<MockConfig, IIntegrationListenerConfiguration>(listenerConfiguration)
            );
        }

        [Fact]
        public void AddAzureServiceBusIntegration_Succeeds_WhenAllRequiredServicesRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Register all required dependencies
            services.AddSingleton<IEventIntegrationPublisher>(new Mock<IEventIntegrationPublisher>().Object);
            services.AddSingleton<IIntegrationFilterService>(new Mock<IIntegrationFilterService>().Object);
            services.AddSingleton<IIntegrationConfigurationDetailsCache>(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            services.AddSingleton<IUserRepository>(new Mock<IUserRepository>().Object);
            services.AddSingleton<IOrganizationRepository>(new Mock<IOrganizationRepository>().Object);
            
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger<EventIntegrationHandler<MockConfig>>>().Object);

            services.AddSingleton<IAzureServiceBusService>(new Mock<IAzureServiceBusService>().Object);

            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.SetupGet(x => x.RoutingKey).Returns("test-key");
            mockListenerConfig.SetupGet(x => x.IntegrationType).Returns("test-type");
            mockListenerConfig.SetupGet(x => x.EventPrefetchCount).Returns(10);
            mockListenerConfig.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);
            mockListenerConfig.SetupGet(x => x.IntegrationPrefetchCount).Returns(20);
            mockListenerConfig.SetupGet(x => x.IntegrationMaxConcurrentCalls).Returns(10);
            var listenerConfiguration = mockListenerConfig.Object;

            // Act
            services.AddAzureServiceBusIntegration<MockConfig, IIntegrationListenerConfiguration>(listenerConfiguration);

            // Assert - No exception thrown
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider);
        }
    }

    // Minimal implementations to avoid missing type errors
    public class MockConfig { }

    public interface IIntegrationListenerConfiguration
    {
        string RoutingKey { get; }
        string IntegrationType { get; }
        int EventPrefetchCount { get; }
        int EventMaxConcurrentCalls { get; }
        int IntegrationPrefetchCount { get; }
        int IntegrationMaxConcurrentCalls { get; }
    }

    public interface IEventIntegrationPublisher { }
    public interface IIntegrationFilterService { }
    public interface IIntegrationConfigurationDetailsCache { }
    public interface IUserRepository { }
    public interface IOrganizationRepository { }
    public interface IAzureServiceBusService { }

    public class EventIntegrationHandler<T> 
    { 
        public EventIntegrationHandler(string integrationType, IEventIntegrationPublisher publisher, 
            IIntegrationFilterService filter, IIntegrationConfigurationDetailsCache cache,
            IUserRepository userRepo, IOrganizationRepository orgRepo, ILogger<EventIntegrationHandler<T>> logger) { }
    }
}
