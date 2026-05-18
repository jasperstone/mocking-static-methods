using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddEventIntegrationHandler_GetRequiredService_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<MockConfiguration>>>();

            services.TryAddSingleton<IEventIntegrationPublisher>(eventIntegrationPublisher.Object);
            services.TryAddSingleton<IIntegrationFilterService>(integrationFilterService.Object);
            services.TryAddSingleton<IIntegrationConfigurationDetailsCache>(configurationCache.Object);
            services.TryAddSingleton<IUserRepository>(userRepository.Object);
            services.TryAddSingleton<IOrganizationRepository>(organizationRepository.Object);
            services.TryAddSingleton<ILogger<EventIntegrationHandler<MockConfiguration>>>(logger.Object);

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var serviceCollectionExtensions = new ServiceCollectionExtensions();
            serviceCollectionExtensions.AddEventIntegrationHandler<MockConfiguration>(services, listenerConfiguration.Object);

            // Assert
            eventIntegrationPublisher.Verify(p => p, Times.Once);
            integrationFilterService.Verify(p => p, Times.Once);
            configurationCache.Verify(p => p, Times.Once);
            userRepository.Verify(p => p, Times.Once);
            organizationRepository.Verify(p => p, Times.Once);
            logger.Verify(p => p, Times.Once);
        }
    }

    public class MockConfiguration : IIntegrationListenerConfiguration
    {
        public string RoutingKey => "MockRoutingKey";
        public IntegrationType IntegrationType => IntegrationType.Mock;
    }
}
