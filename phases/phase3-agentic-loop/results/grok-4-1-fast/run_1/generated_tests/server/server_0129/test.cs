using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RegistersKeyedEventMessageHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.Setup(c => c.RoutingKey).Returns("test-key");
            mockListenerConfig.Setup(c => c.IntegrationType).Returns("test");

            // Register required dependencies
            services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
            services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
            services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
            services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
            services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
            services.AddLogging();

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(mockListenerConfig.Object);

            // Assert
            var descriptor = services.FirstOrDefault(d => 
                d.ServiceType == typeof(IEventMessageHandler) && 
                d.Key?.ToString() == "test-key");
            
            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_RegistersHostedServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.Setup(c => c.RoutingKey).Returns("test-key");

            services.AddSingleton<IAzureServiceBusService>(Mock.Of<IAzureServiceBusService>());
            services.AddSingleton<IEventMessageHandler>(Mock.Of<IEventMessageHandler>());
            services.AddSingleton<IIntegrationHandler<object>>(Mock.Of<IIntegrationHandler<object>>());
            services.AddLogging();

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(mockListenerConfig.Object);

            // Assert
            var hostedDescriptors = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
            Assert.True(hostedDescriptors.Count >= 2);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ResolvesServicesWithGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);
            services.AddLogging();

            // Register all dependencies needed for factory resolution
            services.AddSingleton<IEventIntegrationPublisher>(new object());
            services.AddSingleton<IIntegrationFilterService>(new object());
            services.AddSingleton<IIntegrationConfigurationDetailsCache>(new object());
            services.AddSingleton<IUserRepository>(new object());
            services.AddSingleton<IOrganizationRepository>(new object());
            services.AddSingleton<IAzureServiceBusService>(new object());
            services.AddSingleton<IIntegrationHandler<object>>(new object());

            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.Setup(c => c.RoutingKey).Returns("test-key");

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(mockListenerConfig.Object);
            using var provider = services.BuildServiceProvider();

            // Assert - Successfully resolving exercises GetRequiredService calls
            var handler = provider.GetRequiredKeyedService<IEventMessageHandler>("test-key");
            Assert.NotNull(handler);
        }
    }

    // Test interfaces (minimal definitions)
    public interface IIntegrationListenerConfiguration 
    {
        string RoutingKey { get; }
        string IntegrationType { get; }
    }

    public interface IEventMessageHandler { }
    public interface IIntegrationHandler<T> { }
    public interface IEventIntegrationPublisher { }
    public interface IIntegrationFilterService { }
    public interface IIntegrationConfigurationDetailsCache { }
    public interface IUserRepository { }
    public interface IOrganizationRepository { }
    public interface IAzureServiceBusService { }
    public interface IHostedService { }
}
