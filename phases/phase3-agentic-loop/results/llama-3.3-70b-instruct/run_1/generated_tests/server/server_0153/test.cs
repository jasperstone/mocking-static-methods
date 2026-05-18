using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddEventIntegrationListenerService_WithValidConfiguration_AddsServicesToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(l => l.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(l => l.IntegrationType).Returns("test-integration-type");

            // Act
            services.AddEventIntegrationListenerService(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            Assert.NotNull(eventMessageHandler);
            var rabbitMqEventListenerService = serviceProvider.GetService<RabbitMqEventListenerService>();
            Assert.NotNull(rabbitMqEventListenerService);
            var rabbitMqIntegrationListenerService = serviceProvider.GetService<RabbitMqIntegrationListenerService>();
            Assert.NotNull(rabbitMqIntegrationListenerService);
        }

        [Fact]
        public void AddEventIntegrationListenerService_WithValidConfiguration_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(l => l.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(l => l.IntegrationType).Returns("test-integration-type");
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetService(typeof(IEventIntegrationPublisher))).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IIntegrationFilterService))).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IUserRepository))).Returns(new Mock<IUserRepository>().Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IOrganizationRepository))).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(ILogger<EventIntegrationHandler>))).Returns(new Mock<ILogger<EventIntegrationHandler>>().Object);

            // Act
            services.AddEventIntegrationListenerService(listenerConfiguration.Object);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(IEventIntegrationPublisher)), Times.Once);
            serviceProviderMock.Verify(p => p.GetService(typeof(IIntegrationFilterService)), Times.Once);
            serviceProviderMock.Verify(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.Once);
            serviceProviderMock.Verify(p => p.GetService(typeof(IUserRepository)), Times.Once);
            serviceProviderMock.Verify(p => p.GetService(typeof(IOrganizationRepository)), Times.Once);
            serviceProviderMock.Verify(p => p.GetService(typeof(ILogger<EventIntegrationHandler>)), Times.Once);
        }
    }
}
