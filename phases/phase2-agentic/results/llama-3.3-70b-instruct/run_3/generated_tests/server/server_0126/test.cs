using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_TryAddKeyedSingleton_EventIntegrationHandlerCreated()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("integrationType");

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IEventIntegrationPublisher))).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IIntegrationFilterService))).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IUserRepository))).Returns(new Mock<IUserRepository>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IOrganizationRepository))).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(ILogger<EventIntegrationHandler<MockClass>>))).Returns(new Mock<ILogger<EventIntegrationHandler<MockClass>>>().Object);

            // Act
            services.AddAzureServiceBusIntegration<MockClass, MockListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var eventIntegrationHandler = serviceProvider.Object.GetService<IEventMessageHandler>();
            Assert.NotNull(eventIntegrationHandler);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_TryAddEnumerable_AzureServiceBusEventListenerServiceCreated()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("integrationType");

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IEventIntegrationPublisher))).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IIntegrationFilterService))).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IUserRepository))).Returns(new Mock<IUserRepository>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IOrganizationRepository))).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(ILogger<EventIntegrationHandler<MockClass>>))).Returns(new Mock<ILogger<EventIntegrationHandler<MockClass>>>().Object);

            // Act
            services.AddAzureServiceBusIntegration<MockClass, MockListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var azureServiceBusEventListenerService = serviceProvider.Object.GetService<IHostedService>();
            Assert.NotNull(azureServiceBusEventListenerService);
        }

        private class MockClass { }

        private class MockListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
            public int IntegrationPrefetchCount { get; set; }
            public int IntegrationMaxConcurrentCalls { get; set; }
        }
    }
}
