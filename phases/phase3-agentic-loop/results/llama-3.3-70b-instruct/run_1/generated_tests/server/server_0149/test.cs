using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_AddsEventMessageHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("integrationType");

            services.AddRabbitMqIntegration<MockConfig, MockListenerConfig>(listenerConfiguration.Object);

            // Act
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            Assert.NotNull(eventMessageHandler);
        }

        private class MockConfig { }

        private class MockListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }

            public string IntegrationType { get; set; }
        }
    }
}
