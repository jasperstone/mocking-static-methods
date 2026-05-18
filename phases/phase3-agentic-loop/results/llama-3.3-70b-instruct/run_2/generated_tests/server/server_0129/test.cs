using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ValidListenerConfiguration_AddsServicesToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(l => l.RoutingKey).Returns("routingKey");
            listenerConfiguration.Setup(l => l.IntegrationType).Returns("integrationType");
            listenerConfiguration.Setup(l => l.EventPrefetchCount).Returns(10);
            listenerConfiguration.Setup(l => l.EventMaxConcurrentCalls).Returns(5);

            // Act
            services.AddAzureServiceBusIntegration(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            var azureServiceBusEventListenerService = serviceProvider.GetService<AzureServiceBusEventListenerService>();
            var azureServiceBusIntegrationListenerService = serviceProvider.GetService<AzureServiceBusIntegrationListenerService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(azureServiceBusEventListenerService);
            Assert.NotNull(azureServiceBusIntegrationListenerService);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_InvalidListenerConfiguration_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(l => l.RoutingKey).Returns(null);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddAzureServiceBusIntegration(listenerConfiguration.Object));
        }
    }
}
