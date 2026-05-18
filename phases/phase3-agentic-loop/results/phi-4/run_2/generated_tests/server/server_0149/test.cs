using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(l => l.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(l => l.IntegrationType).Returns("test-integration-type");

            var providerMock = new Mock<IServiceProvider>();
            providerMock.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            providerMock.Setup(p => p.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            providerMock.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            providerMock.Setup(p => p.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            providerMock.Setup(p => p.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            providerMock.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            providerMock.Setup(p => p.GetRequiredService<IRabbitMqService>()).Returns(new Mock<IRabbitMqService>().Object);
            providerMock.Setup(p => p.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);
            providerMock.Setup(p => p.GetRequiredService<TimeProvider>()).Returns(new Mock<TimeProvider>().Object);

            // Act
            ServiceCollectionExtensions.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(services, listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventIntegrationHandler = serviceProvider.GetRequiredService<IEventMessageHandler>(listenerConfiguration.Object.RoutingKey);
            Assert.NotNull(eventIntegrationHandler);

            var rabbitMqEventListenerService = serviceProvider.GetRequiredService<IHostedService>(listenerConfiguration.Object.RoutingKey);
            Assert.NotNull(rabbitMqEventListenerService);

            var rabbitMqIntegrationListenerService = serviceProvider.GetRequiredService<IHostedService>();
            Assert.NotNull(rabbitMqIntegrationListenerService);

            providerMock.Verify(p => p.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IUserRepository>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IOrganizationRepository>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IRabbitMqService>(), Times.Exactly(2));
            providerMock.Verify(p => p.GetRequiredService<ILoggerFactory>(), Times.Exactly(2));
            providerMock.Verify(p => p.GetRequiredService<TimeProvider>(), Times.Once);
        }
    }
}
