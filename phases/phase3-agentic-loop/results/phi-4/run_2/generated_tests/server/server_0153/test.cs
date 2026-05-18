using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceProviderServiceExtensionsTests
    {
        [Fact]
        public void AddIntegrationListener_ShouldCallGetRequiredServiceWithCorrectTypes()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var providerMock = new Mock<IServiceProvider>();

            // Mock GetRequiredService to return a dummy object for each type
            providerMock.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>()).Returns(new object());
            providerMock.Setup(p => p.GetRequiredService<IIntegrationFilterService>()).Returns(new object());
            providerMock.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new object());
            providerMock.Setup(p => p.GetRequiredService<IUserRepository>()).Returns(new object());
            providerMock.Setup(p => p.GetRequiredService<IOrganizationRepository>()).Returns(new object());
            providerMock.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new object());
            providerMock.Setup(p => p.GetRequiredService<IRabbitMqService>()).Returns(new object());
            providerMock.Setup(p => p.GetRequiredService<ILoggerFactory>()).Returns(new object());
            providerMock.Setup(p => p.GetRequiredService<TimeProvider>()).Returns(new object());

            serviceProviderMock.Setup(s => s.GetRequiredService<IServiceProvider>()).Returns(providerMock.Object);

            // Set up the listener configuration mock
            listenerConfiguration.Setup(c => c.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.Setup(c => c.IntegrationType).Returns("test-integration-type");

            // Act
            var result = ServiceProviderServiceExtensions.AddIntegrationListener<object>(
                services,
                serviceProviderMock.Object,
                listenerConfiguration.Object);

            // Assert
            providerMock.Verify(p => p.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IUserRepository>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IOrganizationRepository>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            providerMock.Verify(p => p.GetRequiredService<IRabbitMqService>(), Times.Exactly(2)); // Called twice
            providerMock.Verify(p => p.GetRequiredService<ILoggerFactory>(), Times.Exactly(2)); // Called twice
            providerMock.Verify(p => p.GetRequiredService<TimeProvider>(), Times.Once);
        }
    }
}
