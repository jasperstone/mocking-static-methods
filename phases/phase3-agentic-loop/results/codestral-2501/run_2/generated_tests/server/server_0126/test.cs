using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Entities;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.Core.Vault.Services;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Logging;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockEventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var mockIntegrationFilterService = new Mock<IIntegrationFilterService>();
            var mockIntegrationConfigurationDetailsCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrganizationRepository = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<IntegrationConfig>>>();

            mockProvider.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>()).Returns(mockEventIntegrationPublisher.Object);
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationFilterService>()).Returns(mockIntegrationFilterService.Object);
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(mockIntegrationConfigurationDetailsCache.Object);
            mockProvider.Setup(p => p.GetRequiredService<IUserRepository>()).Returns(mockUserRepository.Object);
            mockProvider.Setup(p => p.GetRequiredService<IOrganizationRepository>()).Returns(mockOrganizationRepository.Object);
            mockProvider.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<IntegrationConfig>>>()).Returns(mockLogger.Object);

            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(c => c.RoutingKey).Returns("testKey");
            listenerConfiguration.Setup(c => c.IntegrationType).Returns(IntegrationType.Event);

            // Act
            serviceCollection.AddAzureServiceBusIntegration<IntegrationConfig, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            Assert.NotNull(eventMessageHandler);
        }
    }

    public class IntegrationConfig { }
}
