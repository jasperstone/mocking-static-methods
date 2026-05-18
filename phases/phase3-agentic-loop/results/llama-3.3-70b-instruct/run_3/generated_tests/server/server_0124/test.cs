using Bit.Core;
using Bit.Core.Auth;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Services;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.Models;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Tools;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("integrationType");

            // Act
            ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, object>(services, listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            Assert.NotNull(eventMessageHandler);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_InvalidConfig_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns(null);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, object>(services, listenerConfiguration.Object));
        }
    }
}
