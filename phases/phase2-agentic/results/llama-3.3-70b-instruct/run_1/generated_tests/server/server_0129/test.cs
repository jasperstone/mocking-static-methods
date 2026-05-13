using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_GetRequiredService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("integrationType");
            listenerConfiguration.SetupGet(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IEventIntegrationPublisher))).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IIntegrationFilterService))).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IUserRepository))).Returns(new Mock<IUserRepository>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IOrganizationRepository))).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(ILogger<EventIntegrationHandler<MockConfiguration>>))).Returns(new Mock<ILogger<EventIntegrationHandler<MockConfiguration>>>().Object);

            // Act
            services.AddAzureServiceBusIntegration<MockConfiguration, MockIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            serviceProvider.Verify(x => x.GetService(typeof(IEventIntegrationPublisher)), Times.Once);
            serviceProvider.Verify(x => x.GetService(typeof(IIntegrationFilterService)), Times.Once);
            serviceProvider.Verify(x => x.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.Once);
            serviceProvider.Verify(x => x.GetService(typeof(IUserRepository)), Times.Once);
            serviceProvider.Verify(x => x.GetService(typeof(IOrganizationRepository)), Times.Once);
            serviceProvider.Verify(x => x.GetService(typeof(ILogger<EventIntegrationHandler<MockConfiguration>>)), Times.Once);
        }

        private class MockConfiguration { }

        private class MockIntegrationListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
        }
    }
}
