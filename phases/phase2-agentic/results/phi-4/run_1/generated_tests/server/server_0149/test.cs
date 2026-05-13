using System;
using System.Collections.Generic;
using System.Linq;
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
            listenerConfiguration.SetupGet(c => c.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(c => c.IntegrationType).Returns("test-integration-type");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IIntegrationFilterService>())
                .Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IUserRepository>())
                .Returns(new Mock<IUserRepository>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IOrganizationRepository>())
                .Returns(new Mock<IOrganizationRepository>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IRabbitMqService>())
                .Returns(new Mock<IRabbitMqService>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(new Mock<ILoggerFactory>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<TimeProvider>())
                .Returns(new Mock<TimeProvider>().Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var eventHandler = provider.GetRequiredService<IEventMessageHandler>();
            Assert.NotNull(eventHandler);

            var rabbitMqEventListenerService = provider.GetServices<IHostedService>().OfType<RabbitMqEventListenerService<IIntegrationListenerConfiguration>>().FirstOrDefault();
            Assert.NotNull(rabbitMqEventListenerService);

            var rabbitMqIntegrationListenerService = provider.GetServices<IHostedService>().OfType<RabbitMqIntegrationListenerService<IIntegrationListenerConfiguration>>().FirstOrDefault();
            Assert.NotNull(rabbitMqIntegrationListenerService);

            serviceProviderMock.Verify(s => s.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IUserRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IOrganizationRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IRabbitMqService>(), Times.Exactly(2));
            serviceProviderMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Exactly(2));
            serviceProviderMock.Verify(s => s.GetRequiredService<TimeProvider>(), Times.Once);
        }
    }
}
