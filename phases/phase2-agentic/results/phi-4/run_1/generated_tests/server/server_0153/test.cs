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
        public void AddIntegrationListenerServices_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(l => l.RoutingKey).Returns("test-routing-key");

            var providerMock = new Mock<IServiceProvider>();
            providerMock.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            providerMock.Setup(p => p.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            providerMock.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            providerMock.Setup(p => p.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            providerMock.Setup(p => p.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            providerMock.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            providerMock.Setup(p => p.GetRequiredService<IRabbitMqService>()).Returns(new Mock<IRabbitMqService>().Object);
            providerMock.Setup(p => p.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);
            providerMock.Setup(p => p.GetRequiredService<TimeProvider>()).Returns(new Mock<TimeProvider>().Object);

            serviceProviderMock.Setup(s => s.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IRabbitMqService>()).Returns(new Mock<IRabbitMqService>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<TimeProvider>()).Returns(new Mock<TimeProvider>().Object);

            // Act
            ServiceCollectionExtensions.AddIntegrationListenerServices(services, serviceProviderMock.Object, listenerConfiguration.Object);

            // Assert
            var serviceDescriptors = services.Where(sd => sd.ServiceType == typeof(IHostedService)).ToList();
            Assert.Equal(2, serviceDescriptors.Count);

            var rabbitMqEventListenerServiceDescriptor = serviceDescriptors.FirstOrDefault(sd => sd.ImplementationType == typeof(RabbitMqEventListenerService<object>));
            Assert.NotNull(rabbitMqEventListenerServiceDescriptor);
            Assert.Equal(ServiceLifetime.Singleton, rabbitMqEventListenerServiceDescriptor.Lifetime);

            var rabbitMqIntegrationListenerServiceDescriptor = serviceDescriptors.FirstOrDefault(sd => sd.ImplementationType == typeof(RabbitMqIntegrationListenerService<object>));
            Assert.NotNull(rabbitMqIntegrationListenerServiceDescriptor);
            Assert.Equal(ServiceLifetime.Singleton, rabbitMqIntegrationListenerServiceDescriptor.Lifetime);
        }
    }
}
