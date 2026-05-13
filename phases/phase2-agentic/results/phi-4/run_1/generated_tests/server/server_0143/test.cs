using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_RegistersServicesCorrectly()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceCollectionMock = new Mock<IServiceCollection>();

            // Mock GetRequiredService calls
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(new Mock<IEventIntegrationPublisher>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IIntegrationFilterService>())
                .Returns(new Mock<IIntegrationFilterService>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IUserRepository>())
                .Returns(new Mock<IUserRepository>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IOrganizationRepository>())
                .Returns(new Mock<IOrganizationRepository>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IRabbitMqService>())
                .Returns(new Mock<IRabbitMqService>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(new Mock<ILoggerFactory>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<TimeProvider>())
                .Returns(new Mock<TimeProvider>().Object);

            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(l => l.RoutingKey).Returns("test-routing-key");

            // Act
            ServiceCollectionExtensions.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(
                serviceCollectionMock.Object, listenerConfiguration.Object);

            // Assert
            serviceCollectionMock.Verify(
                s => s.TryAddKeyedSingleton<IEventMessageHandler>(
                    serviceKey: "test-routing-key",
                    implementationFactory: It.IsAny<Func<IServiceProvider, object, IEventMessageHandler>>()),
                Times.Once);

            serviceCollectionMock.Verify(
                s => s.TryAddEnumerable(
                    It.IsAny<ServiceDescriptor>(),
                    It.IsAny<Func<IServiceProvider, RabbitMqEventListenerService<IIntegrationListenerConfiguration>>>()),
                Times.Once);

            serviceCollectionMock.Verify(
                s => s.TryAddEnumerable(
                    It.IsAny<ServiceDescriptor>(),
                    It.IsAny<Func<IServiceProvider, RabbitMqIntegrationListenerService<IIntegrationListenerConfiguration>>>()),
                Times.Once);
        }
    }
}
