using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Repositories;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using Bit.Core.AdminConsole.Models.Teams;
using Bit.Core.Platform.PushRegistration.Internal;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockListenerConfiguration = new Mock<IIntegrationListenerConfiguration>();

            mockListenerConfiguration.Setup(c => c.RoutingKey).Returns("testKey");
            mockListenerConfiguration.Setup(c => c.IntegrationType).Returns(IntegrationType.Event);
            mockListenerConfiguration.Setup(c => c.EventPrefetchCount).Returns(10);
            mockListenerConfiguration.Setup(c => c.EventMaxConcurrentCalls).Returns(5);

            mockServiceProvider.Setup(sp => sp.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IIntegrationHandler<object>>()).Returns(new Mock<IIntegrationHandler<object>>().Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IAzureServiceBusService>()).Returns(new Mock<IAzureServiceBusService>().Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(mockListenerConfiguration.Object);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            var hostedService = serviceProvider.GetRequiredService<IHostedService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(hostedService);
        }
    }
}
