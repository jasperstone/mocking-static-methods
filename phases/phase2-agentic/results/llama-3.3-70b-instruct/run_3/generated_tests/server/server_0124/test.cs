using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddAzureServiceBusIntegration_ResolvesDependenciesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("integrationType");

            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<object>>>();

            services.AddSingleton(eventIntegrationPublisher.Object);
            services.AddSingleton(integrationFilterService.Object);
            services.AddSingleton(configurationCache.Object);
            services.AddSingleton(userRepository.Object);
            services.AddSingleton(organizationRepository.Object);
            services.AddSingleton(logger.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventIntegrationHandler = serviceProvider.GetRequiredService<IEventMessageHandler>("routingKey");

            Assert.IsType<EventIntegrationHandler<object>>(eventIntegrationHandler);
            Assert.Same(eventIntegrationPublisher.Object, ((EventIntegrationHandler<object>)eventIntegrationHandler)._eventIntegrationPublisher);
            Assert.Same(integrationFilterService.Object, ((EventIntegrationHandler<object>)eventIntegrationHandler)._integrationFilterService);
            Assert.Same(configurationCache.Object, ((EventIntegrationHandler<object>)eventIntegrationHandler)._configurationCache);
            Assert.Same(userRepository.Object, ((EventIntegrationHandler<object>)eventIntegrationHandler)._userRepository);
            Assert.Same(organizationRepository.Object, ((EventIntegrationHandler<object>)eventIntegrationHandler)._organizationRepository);
            Assert.Same(logger.Object, ((EventIntegrationHandler<object>)eventIntegrationHandler)._logger);
        }
    }
}
