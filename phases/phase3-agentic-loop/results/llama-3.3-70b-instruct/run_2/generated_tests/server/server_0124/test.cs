using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddAzureServiceBusIntegration_GetRequiredService_Called()
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

            services.AddSingleton<IEventIntegrationPublisher>(eventIntegrationPublisher.Object);
            services.AddSingleton<IIntegrationFilterService>(integrationFilterService.Object);
            services.AddSingleton<IIntegrationConfigurationDetailsCache>(configurationCache.Object);
            services.AddSingleton<IUserRepository>(userRepository.Object);
            services.AddSingleton<IOrganizationRepository>(organizationRepository.Object);
            services.AddSingleton<ILogger<EventIntegrationHandler<object>>>(logger.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, object>(listenerConfiguration.Object);

            // Assert
            eventIntegrationPublisher.Verify(x => x, Times.Once);
        }
    }
}
