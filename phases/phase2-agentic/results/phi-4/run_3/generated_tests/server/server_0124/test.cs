using System;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RequestsCorrectServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockProvider
                .Setup(p => p.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(new Mock<IEventIntegrationPublisher>().Object);
            mockProvider
                .Setup(p => p.GetRequiredService<IIntegrationFilterService>())
                .Returns(new Mock<IIntegrationFilterService>().Object);
            mockProvider
                .Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            mockProvider
                .Setup(p => p.GetRequiredService<IUserRepository>())
                .Returns(new Mock<IUserRepository>().Object);
            mockProvider
                .Setup(p => p.GetRequiredService<IOrganizationRepository>())
                .Returns(new Mock<IOrganizationRepository>().Object);
            mockProvider
                .Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(Mock.Of<ILogger<EventIntegrationHandler<object>>>());
            mockProvider
                .Setup(p => p.GetRequiredService<IAzureServiceBusService>())
                .Returns(new Mock<IAzureServiceBusService>().Object);
            mockProvider
                .Setup(p => p.GetRequiredService<ILoggerFactory>())
                .Returns(Mock.Of<ILoggerFactory>());

            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(l => l.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(l => l.IntegrationType).Returns("test-integration-type");
            listenerConfiguration.SetupGet(l => l.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(l => l.EventMaxConcurrentCalls).Returns(5);

            // Act
            ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(services, listenerConfiguration.Object);

            // Assert
            mockProvider.Verify(p => p.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IUserRepository>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IOrganizationRepository>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IAzureServiceBusService>(), Times.Exactly(2));
            mockProvider.Verify(p => p.GetRequiredService<ILoggerFactory>(), Times.Once);
        }
    }
}
