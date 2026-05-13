using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("integrationType");
            listenerConfiguration.SetupGet(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);
            listenerConfiguration.SetupGet(x => x.IntegrationPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.IntegrationMaxConcurrentCalls).Returns(5);

            // Act
            services.AddAzureServiceBusIntegration(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventIntegrationHandler = serviceProvider.GetRequiredService<IEventMessageHandler>("routingKey");
            var azureServiceBusEventListenerService = serviceProvider.GetRequiredService<AzureServiceBusEventListenerService>();
            var azureServiceBusIntegrationListenerService = serviceProvider.GetRequiredService<AzureServiceBusIntegrationListenerService>();

            Assert.NotNull(eventIntegrationHandler);
            Assert.NotNull(azureServiceBusEventListenerService);
            Assert.NotNull(azureServiceBusIntegrationListenerService);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ThrowsException_WhenServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("integrationType");
            listenerConfiguration.SetupGet(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);
            listenerConfiguration.SetupGet(x => x.IntegrationPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.IntegrationMaxConcurrentCalls).Returns(5);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddAzureServiceBusIntegration(listenerConfiguration.Object));
        }
    }
}
