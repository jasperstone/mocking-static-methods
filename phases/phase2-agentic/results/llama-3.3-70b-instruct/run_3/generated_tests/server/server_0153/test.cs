using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddEventIntegrationListener_WithValidListenerConfiguration_AddsServicesToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new ListenerConfiguration
            {
                RoutingKey = "test-routing-key",
                IntegrationType = "test-integration-type"
            };

            // Act
            services.AddEventIntegrationListener(listenerConfiguration);

            // Assert
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IEventMessageHandler));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(RabbitMqEventListenerService<>));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(RabbitMqIntegrationListenerService<>));
        }

        [Fact]
        public void AddEventIntegrationListener_WithInvalidListenerConfiguration_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new ListenerConfiguration
            {
                RoutingKey = null,
                IntegrationType = "test-integration-type"
            };

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddEventIntegrationListener(listenerConfiguration));
        }

        [Fact]
        public void GetRequiredService_IsCalledOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var listenerConfiguration = new ListenerConfiguration
            {
                RoutingKey = "test-routing-key",
                IntegrationType = "test-integration-type"
            };

            // Act
            services.AddEventIntegrationListener(listenerConfiguration);

            // Assert
            var eventIntegrationHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            Assert.NotNull(eventIntegrationHandler);
        }
    }

    public class ListenerConfiguration
    {
        public string RoutingKey { get; set; }
        public string IntegrationType { get; set; }
    }
}
