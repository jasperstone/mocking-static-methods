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
            listenerConfiguration.SetupGet(x => x.IntegrationPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.IntegrationMaxConcurrentCalls).Returns(5);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProvider.Setup(x => x.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProvider.Setup(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProvider.Setup(x => x.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            serviceProvider.Setup(x => x.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProvider.Setup(x => x.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            serviceProvider.Setup(x => x.GetRequiredService<IAzureServiceBusService>()).Returns(new Mock<IAzureServiceBusService>().Object);
            serviceProvider.Setup(x => x.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            serviceProvider.Verify(x => x.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            serviceProvider.Verify(x => x.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            serviceProvider.Verify(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            serviceProvider.Verify(x => x.GetRequiredService<IUserRepository>(), Times.Once);
            serviceProvider.Verify(x => x.GetRequiredService<IOrganizationRepository>(), Times.Once);
            serviceProvider.Verify(x => x.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            serviceProvider.Verify(x => x.GetRequiredService<IAzureServiceBusService>(), Times.Exactly(2));
            serviceProvider.Verify(x => x.GetRequiredService<ILoggerFactory>(), Times.Exactly(2));
        }
    }
}
