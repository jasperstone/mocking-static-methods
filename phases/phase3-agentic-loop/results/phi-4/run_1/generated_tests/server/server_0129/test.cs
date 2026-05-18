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
        public void AddAzureServiceBusIntegration_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(l => l.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(l => l.IntegrationType).Returns("test-integration-type");
            listenerConfiguration.SetupGet(l => l.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(l => l.EventMaxConcurrentCalls).Returns(5);
            listenerConfiguration.SetupGet(l => l.IntegrationPrefetchCount).Returns(20);
            listenerConfiguration.SetupGet(l => l.IntegrationMaxConcurrentCalls).Returns(10);

            var serviceProviderMock = new Mock<IServiceProvider>();
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
                .Setup(s => s.GetRequiredService<IAzureServiceBusService>())
                .Returns(new Mock<IAzureServiceBusService>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(new Mock<ILoggerFactory>().Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var eventMessageHandlerDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEventMessageHandler) && d.ImplementationFactory != null);
            Assert.NotNull(eventMessageHandlerDescriptor);
            var hostedServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHostedService) && d.ImplementationFactory != null);
            Assert.NotNull(hostedServiceDescriptor);

            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = eventMessageHandlerDescriptor.ImplementationFactory(serviceProvider, null);
            Assert.NotNull(eventMessageHandler);

            var hostedService = hostedServiceDescriptor.ImplementationFactory(serviceProvider);
            Assert.NotNull(hostedService);

            serviceProviderMock.Verify(s => s.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IUserRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IOrganizationRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IAzureServiceBusService>(), Times.Exactly(2));
            serviceProviderMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
        }
    }
}
