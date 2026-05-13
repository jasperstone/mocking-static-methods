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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(l => l.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(l => l.IntegrationType).Returns("test-integration-type");
            listenerConfiguration.SetupGet(l => l.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(l => l.EventMaxConcurrentCalls).Returns(5);
            listenerConfiguration.SetupGet(l => l.IntegrationPrefetchCount).Returns(20);
            listenerConfiguration.SetupGet(l => l.IntegrationMaxConcurrentCalls).Returns(10);

            // Mock GetRequiredService calls
            serviceProviderMock.Setup(s => s.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IAzureServiceBusService>()).Returns(new Mock<IAzureServiceBusService>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IIntegrationHandler<object>>()).Returns(new Mock<IIntegrationHandler<object>>().Object);

            // Act
            ServiceCollectionExtensions.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(services, listenerConfiguration.Object);

            // Assert
            var serviceDescriptors = services.Where(sd => sd.ServiceType == typeof(IEventMessageHandler)).ToList();
            Assert.Single(serviceDescriptors);
            var keyedSingleton = serviceDescriptors[0] as ServiceDescriptor;
            Assert.NotNull(keyedSingleton);
            Assert.Equal(ServiceLifetime.Singleton, keyedSingleton.Lifetime);

            var hostedServices = services.Where(sd => sd.ServiceType == typeof(IHostedService)).ToList();
            Assert.Equal(2, hostedServices.Count);

            var azureServiceBusEventListenerService = hostedServices[0] as ServiceDescriptor;
            Assert.NotNull(azureServiceBusEventListenerService);
            Assert.Equal(ServiceLifetime.Singleton, azureServiceBusEventListenerService.Lifetime);

            var azureServiceBusIntegrationListenerService = hostedServices[1] as ServiceDescriptor;
            Assert.NotNull(azureServiceBusIntegrationListenerService);
            Assert.Equal(ServiceLifetime.Singleton, azureServiceBusIntegrationListenerService.Lifetime);
        }
    }
}
