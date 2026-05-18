using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Core.Services;
using Bit.Core.Repositories;
using Bit.Core.Entities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_ValidConfig_AddsServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("test-integration-type");

            // Act
            services.AddRabbitMqIntegration<MockIntegrationConfigurationDetails, MockListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            Assert.NotNull(eventMessageHandler);
            var rabbitMqEventListenerService = serviceProvider.GetService<RabbitMqEventListenerService<MockListenerConfiguration>>();
            Assert.NotNull(rabbitMqEventListenerService);
            var rabbitMqIntegrationListenerService = serviceProvider.GetService<RabbitMqIntegrationListenerService<MockListenerConfiguration>>();
            Assert.NotNull(rabbitMqIntegrationListenerService);
        }

        [Fact]
        public void AddRabbitMqIntegration_InvalidConfig_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns(null);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddRabbitMqIntegration<MockIntegrationConfigurationDetails, MockListenerConfiguration>(listenerConfiguration.Object));
        }

        private class MockIntegrationConfigurationDetails : IIntegrationConfigurationDetails
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        private class MockListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
        }
    }
}
