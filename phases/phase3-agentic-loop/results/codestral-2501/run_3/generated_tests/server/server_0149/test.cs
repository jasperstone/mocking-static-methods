using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Platform;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.Core.Utilities;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_ShouldAddServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(x => x.RoutingKey).Returns("testRoutingKey");
            listenerConfiguration.Setup(x => x.IntegrationType).Returns("testIntegrationType");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IEventIntegrationPublisher>()).Returns(Mock.Of<IEventIntegrationPublisher>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationFilterService>()).Returns(Mock.Of<IIntegrationFilterService>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IUserRepository>()).Returns(Mock.Of<IUserRepository>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IOrganizationRepository>()).Returns(Mock.Of<IOrganizationRepository>());
            serviceProviderMock.Setup(x => x.GetRequiredService<ILogger<EventIntegrationHandler<WebhookIntegrationConfigurationDetails>>>()).Returns(Mock.Of<ILogger<EventIntegrationHandler<WebhookIntegrationConfigurationDetails>>>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IRabbitMqService>()).Returns(Mock.Of<IRabbitMqService>());
            serviceProviderMock.Setup(x => x.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(x => x.GetRequiredService<TimeProvider>()).Returns(Mock.Of<TimeProvider>());
            serviceProviderMock.Setup(x => x.GetRequiredKeyedService<IEventMessageHandler>("testRoutingKey")).Returns(Mock.Of<IEventMessageHandler>());

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddRabbitMqIntegration<WebhookIntegrationConfigurationDetails, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            var rabbitMqEventListenerService = serviceProvider.GetRequiredService<IHostedService>();
            var rabbitMqIntegrationListenerService = serviceProvider.GetRequiredService<IHostedService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(rabbitMqEventListenerService);
            Assert.NotNull(rabbitMqIntegrationListenerService);

            serviceProviderMock.Verify(x => x.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IUserRepository>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IOrganizationRepository>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<ILogger<EventIntegrationHandler<WebhookIntegrationConfigurationDetails>>>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IRabbitMqService>(), Times.Exactly(2));
            serviceProviderMock.Verify(x => x.GetRequiredService<ILoggerFactory>(), Times.Exactly(2));
            serviceProviderMock.Verify(x => x.GetRequiredService<TimeProvider>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredKeyedService<IEventMessageHandler>("testRoutingKey"), Times.Once);
        }
    }
}
