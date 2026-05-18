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

            // Act
            services.AddAzureServiceBusIntegration<object, object>(listenerConfiguration.Object);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IEventMessageHandler));
            Assert.Contains(services, s => s.ServiceType == typeof(AzureServiceBusEventListenerService<object>));
            Assert.Contains(services, s => s.ServiceType == typeof(AzureServiceBusIntegrationListenerService<object>));
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ThrowsException_WhenListenerConfigurationIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            IIntegrationListenerConfiguration? listenerConfiguration = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddAzureServiceBusIntegration<object, object>(listenerConfiguration));
        }
    }
}
