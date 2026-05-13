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
            listenerConfiguration.Setup(c => c.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.Setup(c => c.IntegrationType).Returns("test-integration-type");

            var providerMock = new Mock<IServiceProvider>();
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var rabbitMqServiceMock = new Mock<IRabbitMqService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var timeProviderMock = new Mock<TimeProvider>();

            providerMock
                .Setup(p => p.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(eventIntegrationPublisherMock.Object);

            providerMock
                .Setup(p => p.GetRequiredService<IIntegrationFilterService>())
                .Returns(integrationFilterServiceMock.Object);

            providerMock
                .Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(configurationCacheMock.Object);

            providerMock
                .Setup(p => p.GetRequiredService<IUserRepository>())
                .Returns(userRepositoryMock.Object);

            providerMock
                .Setup(p => p.GetRequiredService<IOrganizationRepository>())
                .Returns(organizationRepositoryMock.Object);

            providerMock
                .Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(loggerMock.Object);

            providerMock
                .Setup(p => p.GetRequiredService<IRabbitMqService>())
                .Returns(rabbitMqServiceMock.Object);

            providerMock
                .Setup(p => p.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            providerMock
                .Setup(p => p.GetRequiredService<TimeProvider>())
                .Returns(timeProviderMock.Object);

            // Act
            services.AddRabbitMqIntegration<object, object>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();

            Assert.NotNull(eventMessageHandler);
            Assert.IsType<EventIntegrationHandler<object>>(eventMessageHandler);

            var rabbitMqEventListenerService = serviceProvider.GetRequiredService<IHostedService>();
            Assert.IsType<RabbitMqEventListenerService<object>>(rabbitMqEventListenerService);

            var rabbitMqIntegrationListenerService = serviceProvider.GetRequiredService<IHostedService>();
            Assert.IsType<RabbitMqIntegrationListenerService<object>>(rabbitMqIntegrationListenerService);
        }
    }
}
