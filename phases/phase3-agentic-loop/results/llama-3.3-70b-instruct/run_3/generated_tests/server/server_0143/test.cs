using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Core.Services;
using Bit.Core.Repositories;
using Bit.Core.Entities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_ServiceProvider_GetRequiredService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<object>>>();

            services.TryAddSingleton<IEventIntegrationPublisher>(eventIntegrationPublisher.Object);
            services.TryAddSingleton<IIntegrationFilterService>(integrationFilterService.Object);
            services.TryAddSingleton<IIntegrationConfigurationDetailsCache>(configurationCache.Object);
            services.TryAddSingleton<IUserRepository>(userRepository.Object);
            services.TryAddSingleton<IOrganizationRepository>(organizationRepository.Object);
            services.TryAddSingleton<ILogger<EventIntegrationHandler<object>>>(logger.Object);

            // Act
            services.AddRabbitMqIntegration<object, object>(listenerConfiguration.Object);

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
