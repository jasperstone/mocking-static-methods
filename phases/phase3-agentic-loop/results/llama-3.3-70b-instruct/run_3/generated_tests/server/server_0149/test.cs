using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.SharedWeb.Utilities;
using System.Threading.Tasks;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddRabbitMqIntegration_ValidConfiguration_AddsServices()
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
            services.AddRabbitMqIntegration<object, object>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            Assert.NotNull(eventMessageHandler);
        }
    }
}
