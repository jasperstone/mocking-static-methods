using System;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RequestsCorrectServices()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var serviceBusService = new Mock<IAzureServiceBusService>();

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(eventIntegrationPublisher.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IIntegrationFilterService>())
                .Returns(integrationFilterService.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(configurationCache.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IUserRepository>())
                .Returns(userRepository.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IOrganizationRepository>())
                .Returns(organizationRepository.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(logger.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IAzureServiceBusService>())
                .Returns(serviceBusService.Object);

            var services = new ServiceCollection();
            services.AddSingleton(mockServiceProvider.Object);

            // Act
            var extensions = new ServiceCollectionExtensions();
            extensions.AddAzureServiceBusIntegration<object, object>(services, listenerConfiguration.Object);

            // Assert
            mockServiceProvider.Verify(s => s.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IUserRepository>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IOrganizationRepository>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IAzureServiceBusService>(), Times.Once);
        }
    }
}
