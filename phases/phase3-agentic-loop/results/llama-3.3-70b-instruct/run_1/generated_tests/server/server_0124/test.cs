using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_AddsServicesToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("integrationType");

            // Act
            services.AddAzureServiceBusIntegration<object, object>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService(typeof(IEventMessageHandler)) as IEventMessageHandler;
            Assert.NotNull(eventMessageHandler);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ThrowsException_WhenListenerConfigurationIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            IIntegrationListenerConfiguration listenerConfiguration = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddAzureServiceBusIntegration<object, object>(listenerConfiguration));
        }
    }
}
