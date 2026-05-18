using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
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
            var listenerConfigurationMock = new Mock<IIntegrationListenerConfiguration>();
            var eventIntegrationPublisherMock = Mock.Of<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = Mock.Of<IIntegrationFilterService>();
            var configurationCacheMock = Mock.Of<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = Mock.Of<IUserRepository>();
            var organizationRepositoryMock = Mock.Of<IOrganizationRepository>();
            var loggerMock = Mock.Of<ILogger<EventIntegrationHandler<Mock.Of<IIntegrationListenerConfiguration>>>>();
            var rabbitMqServiceMock = Mock.Of<IRabbitMqService>();
            var loggerFactoryMock = Mock.Of<ILoggerFactory>();
            var timeProviderMock = Mock.Of<TimeProvider>();

            // Setup the mock to return the same provider for GetRequiredService calls
            serviceProviderMock
                .Setup(p => p.GetRequiredService(typeof(IEventIntegrationPublisher)))
                .Returns(eventIntegrationPublisherMock);
            serviceProviderMock
                .Setup(p => p.GetRequiredService(typeof(IIntegrationFilterService)))
                .Returns(integrationFilterServiceMock);
            serviceProviderMock
                .Setup(p => p.GetRequiredService(typeof(IIntegrationConfigurationDetailsCache)))
                .Returns(configurationCacheMock);
            serviceProviderMock
                .Setup(p => p.GetRequiredService(typeof(IUserRepository)))
                .Returns(userRepositoryMock);
            serviceProviderMock
                .Setup(p => p.GetRequiredService(typeof(IOrganizationRepository)))
                .Returns(organizationRepositoryMock);
            serviceProviderMock
                .Setup(p => p.GetRequiredService(typeof(ILogger<EventIntegrationHandler<Mock.Of<IIntegrationListenerConfiguration>>>)))
                .Returns(loggerMock);
            serviceProviderMock
                .Setup(p => p.GetRequiredService(typeof(IRabbitMqService)))
                .Returns(rabbitMqServiceMock);
            serviceProviderMock
                .Setup(p => p.GetRequiredService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock);
            serviceProviderMock
                .Setup(p => p.GetRequiredService(typeof(TimeProvider)))
                .Returns(timeProviderMock);

            // Act
            var result = ServiceProviderServiceExtensions
                .AddIntegrationListener(services, listenerConfigurationMock.Object, serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(
                p => p.GetRequiredService(typeof(IEventIntegrationPublisher)), Times.Once);
            serviceProviderMock.Verify(
                p => p.GetRequiredService(typeof(IIntegrationFilterService)), Times.Once);
            serviceProviderMock.Verify(
                p => p.GetRequiredService(typeof(IIntegrationConfigurationDetailsCache)), Times.Once);
            serviceProviderMock.Verify(
                p => p.GetRequiredService(typeof(IUserRepository)), Times.Once);
            serviceProviderMock.Verify(
                p => p.GetRequiredService(typeof(IOrganizationRepository)), Times.Once);
            serviceProviderMock.Verify(
                p => p.GetRequiredService(typeof(ILogger<EventIntegrationHandler<Mock.Of<IIntegrationListenerConfiguration>>>)), Times.Once);
            serviceProviderMock.Verify(
                p => p.GetRequiredService(typeof(IRabbitMqService)), Times.Exactly(2)); // Called twice
            serviceProviderMock.Verify(
                p => p.GetRequiredService(typeof(ILoggerFactory)), Times.Exactly(2)); // Called twice
            serviceProviderMock.Verify(
                p => p.GetRequiredService(typeof(TimeProvider)), Times.Once);
        }
    }
}
