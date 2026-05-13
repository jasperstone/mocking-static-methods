using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            services.AddRabbitMqIntegration<MockConfig, MockListenerConfig>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            Assert.NotNull(eventMessageHandler);
            var rabbitMqEventListenerService = serviceProvider.GetService<RabbitMqEventListenerService<MockListenerConfig>>();
            Assert.NotNull(rabbitMqEventListenerService);
            var rabbitMqIntegrationListenerService = serviceProvider.GetService<RabbitMqIntegrationListenerService<MockListenerConfig>>();
            Assert.NotNull(rabbitMqIntegrationListenerService);
        }

        [Fact]
        public void AddRabbitMqIntegration_GetRequiredService_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("test-integration-type");
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<ILogger<EventIntegrationHandler<MockConfig>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<MockConfig>>>().Object);

            // Act
            services.AddRabbitMqIntegration<MockConfig, MockListenerConfig>(listenerConfiguration.Object);

            // Assert
            serviceProviderMock.Verify(x => x.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IUserRepository>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IOrganizationRepository>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<ILogger<EventIntegrationHandler<MockConfig>>>(), Times.Once);
        }
    }

    public class MockConfig { }

    public class MockListenerConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey { get; set; }
        public string IntegrationType { get; set; }
    }
}
