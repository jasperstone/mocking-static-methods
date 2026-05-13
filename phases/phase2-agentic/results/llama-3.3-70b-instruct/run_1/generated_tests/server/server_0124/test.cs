using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_GetRequiredServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns(typeof(string));
            listenerConfiguration.SetupGet(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);

            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<string>>>();

            services.AddSingleton(eventIntegrationPublisher.Object);
            services.AddSingleton(integrationFilterService.Object);
            services.AddSingleton(configurationCache.Object);
            services.AddSingleton(userRepository.Object);
            services.AddSingleton(organizationRepository.Object);
            services.AddSingleton(logger.Object);

            // Act
            services.AddAzureServiceBusIntegration<string, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            eventIntegrationPublisher.Verify(x => x, Times.Once);
            integrationFilterService.Verify(x => x, Times.Once);
            configurationCache.Verify(x => x, Times.Once);
            userRepository.Verify(x => x, Times.Once);
            organizationRepository.Verify(x => x, Times.Once);
            logger.Verify(x => x, Times.Once);
        }
    }
}
