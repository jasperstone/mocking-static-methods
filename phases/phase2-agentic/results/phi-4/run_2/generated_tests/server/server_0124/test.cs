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
            var mockServiceProvider = new Mock<IServiceProvider>();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var services = new ServiceCollection();

            // Mock the GetRequiredService calls
            mockServiceProvider
                .Setup(s => s.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(new Mock<IEventIntegrationPublisher>().Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IIntegrationFilterService>())
                .Returns(new Mock<IIntegrationFilterService>().Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IUserRepository>())
                .Returns(new Mock<IUserRepository>().Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IOrganizationRepository>())
                .Returns(new Mock<IOrganizationRepository>().Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(Mock.Of<ILogger<EventIntegrationHandler<object>>>());

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IAzureServiceBusService>())
                .Returns(new Mock<IAzureServiceBusService>().Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(Mock.Of<ILoggerFactory>());

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IIntegrationHandler<object>>())
                .Returns(new Mock<IIntegrationHandler<object>>().Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            mockServiceProvider.Verify(s => s.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IUserRepository>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IOrganizationRepository>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IAzureServiceBusService>(), Times.Exactly(2));
            mockServiceProvider.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService<IIntegrationHandler<object>>(), Times.Once);
        }
    }
}
