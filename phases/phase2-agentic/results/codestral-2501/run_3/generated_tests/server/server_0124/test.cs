using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.IServiceProvider;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_Should_RegisterServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var mockIntegrationFilterService = new Mock<IIntegrationFilterService>();
            var mockIntegrationConfigurationDetailsCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrganizationRepository = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<IEventIntegrationPublisher>()).Returns(mockEventIntegrationPublisher.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IIntegrationFilterService>()).Returns(mockIntegrationFilterService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(mockIntegrationConfigurationDetailsCache.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IUserRepository>()).Returns(mockUserRepository.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOrganizationRepository>()).Returns(mockOrganizationRepository.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(mockLogger.Object);

            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>().Object;

            // Act
            serviceCollection.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            Assert.NotNull(eventMessageHandler);
        }
    }
}
