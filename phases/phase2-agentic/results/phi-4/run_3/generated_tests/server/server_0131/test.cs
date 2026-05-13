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

            var providerMock = new Mock<IServiceProvider>();
            providerMock.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            providerMock.Setup(p => p.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            providerMock.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            providerMock.Setup(p => p.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            providerMock.Setup(p => p.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            providerMock.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            providerMock.Setup(p => p.GetRequiredService<IAzureServiceBusService>()).Returns(new Mock<IAzureServiceBusService>().Object);
            providerMock.Setup(p => p.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);
            providerMock.Setup(p => p.GetRequiredService<IIntegrationHandler<object>>()).Returns(new Mock<IIntegrationHandler<object>>().Object);

            // Act
            services.AddAzureServiceBusIntegration<object, object>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IKeyedService<IEventMessageHandler>>("test-routing-key");
            var azureServiceBusEventListenerService = serviceProvider.GetRequiredService<IHostedService>();
            var azureServiceBusIntegrationListenerService = serviceProvider.GetRequiredService<IHostedService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(azureServiceBusEventListenerService);
            Assert.NotNull(azureServiceBusIntegrationListenerService);
        }
    }
}
