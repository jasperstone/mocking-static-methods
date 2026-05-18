using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Core.Utilities;
using Bit.Core.Services;
using Bit.Core.Repositories;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ValidConfig_AddsServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("test-integration-type");
            listenerConfiguration.SetupGet(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);
            listenerConfiguration.SetupGet(x => x.IntegrationPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.IntegrationMaxConcurrentCalls).Returns(5);

            services.AddAzureServiceBusIntegration(listenerConfiguration.Object);

            // Act
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            var azureServiceBusEventListenerService = serviceProvider.GetService<AzureServiceBusEventListenerService>();
            var azureServiceBusIntegrationListenerService = serviceProvider.GetService<AzureServiceBusIntegrationListenerService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(azureServiceBusEventListenerService);
            Assert.NotNull(azureServiceBusIntegrationListenerService);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_InvalidConfig_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns(null);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddAzureServiceBusIntegration(listenerConfiguration.Object));
        }
    }
}
