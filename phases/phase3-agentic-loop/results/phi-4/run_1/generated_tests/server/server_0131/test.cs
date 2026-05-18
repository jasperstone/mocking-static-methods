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
                .Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(new Mock<ILoggerFactory>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IAzureServiceBusService>())
                .Returns(new Mock<IAzureServiceBusService>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IEventMessageHandler>(It.IsAny<object>()))
                .Returns(new Mock<IEventMessageHandler>().Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IIntegrationHandler<object>>())
                .Returns(new Mock<IIntegrationHandler<object>>().Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var eventIntegrationHandler = provider.GetRequiredService<IEventMessageHandler>("test-routing-key");
            Assert.NotNull(eventIntegrationHandler);

            var azureServiceBusEventListenerService = provider.GetServices<IHostedService>().OfType<AzureServiceBusEventListenerService<IIntegrationListenerConfiguration>>().FirstOrDefault();
            Assert.NotNull(azureServiceBusEventListenerService);

            var azureServiceBusIntegrationListenerService = provider.GetServices<IHostedService>().OfType<AzureServiceBusIntegrationListenerService<IIntegrationListenerConfiguration>>().FirstOrDefault();
            Assert.NotNull(azureServiceBusIntegrationListenerService);

            serviceProviderMock.Verify(s => s.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IUserRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IOrganizationRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Exactly(2));
            serviceProviderMock.Verify(s => s.GetRequiredService<IAzureServiceBusService>(), Times.Exactly(2));
            serviceProviderMock.Verify(s => s.GetRequiredService<IEventMessageHandler>(It.IsAny<object>()), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationHandler<object>>(), Times.Once);
        }
    }

    // Mocked types for compilation
    public interface IIntegrationListenerConfiguration
    {
        string RoutingKey { get; }
        string IntegrationType { get; }
        int EventPrefetchCount { get; }
        int EventMaxConcurrentCalls { get; }
        int IntegrationPrefetchCount { get; }
        int IntegrationMaxConcurrentCalls { get; }
    }

    public interface IEventIntegrationPublisher { }
    public interface IIntegrationFilterService { }
    public interface IIntegrationConfigurationDetailsCache { }
    public interface IUserRepository { }
    public interface IOrganizationRepository { }
    public interface ILogger<T> { }
    public interface ILoggerFactory { }
    public interface IAzureServiceBusService { }
    public interface IEventMessageHandler { }
    public interface IIntegrationHandler<T> { }
    public interface IHostedService { }
    public interface EventIntegrationHandler<T> { }

    public class AzureServiceBusEventListenerService<T> : IHostedService { }
    public class AzureServiceBusIntegrationListenerService<T> : IHostedService { }
}
