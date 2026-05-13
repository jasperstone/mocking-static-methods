using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>().Object;

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IEventMessageHandler>());
            Assert.NotNull(serviceProvider.GetService<AzureServiceBusEventListenerService<IIntegrationListenerConfiguration>>());
            Assert.NotNull(serviceProvider.GetService<AzureServiceBusIntegrationListenerService<object>>());
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ThrowsException_WhenListenerConfigurationIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            IIntegrationListenerConfiguration? listenerConfiguration = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration));
        }
    }
}
