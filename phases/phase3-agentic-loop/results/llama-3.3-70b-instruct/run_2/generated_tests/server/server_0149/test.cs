using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_AddsIEventMessageHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();

            // Act
            services.AddRabbitMqIntegration<WebhookIntegrationConfigurationDetails, HecListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService(typeof(IEventMessageHandler)) as IEventMessageHandler;
            Assert.NotNull(eventMessageHandler);
        }

        [Fact]
        public void AddRabbitMqIntegration_AddsRabbitMqEventListenerService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();

            // Act
            services.AddRabbitMqIntegration<WebhookIntegrationConfigurationDetails, HecListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var rabbitMqEventListenerService = serviceProvider.GetService(typeof(RabbitMqEventListenerService<HecListenerConfiguration>)) as RabbitMqEventListenerService<HecListenerConfiguration>;
            Assert.NotNull(rabbitMqEventListenerService);
        }

        [Fact]
        public void AddRabbitMqIntegration_AddsRabbitMqIntegrationListenerService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();

            // Act
            services.AddRabbitMqIntegration<WebhookIntegrationConfigurationDetails, HecListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var rabbitMqIntegrationListenerService = serviceProvider.GetService(typeof(RabbitMqIntegrationListenerService<HecListenerConfiguration>)) as RabbitMqIntegrationListenerService<HecListenerConfiguration>;
            Assert.NotNull(rabbitMqIntegrationListenerService);
        }
    }
}
