using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ValidConfig_AddsServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("test-routing-key");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns("test-integration-type");
            listenerConfiguration.SetupGet(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);
            listenerConfiguration.SetupGet(x => x.IntegrationPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.IntegrationMaxConcurrentCalls).Returns(5);

            services.TryAddSingleton<IEventIntegrationPublisher, MockEventIntegrationPublisher>();
            services.TryAddSingleton<IIntegrationFilterService, MockIntegrationFilterService>();
            services.TryAddSingleton<IIntegrationConfigurationDetailsCache, MockIntegrationConfigurationDetailsCache>();
            services.TryAddSingleton<IUserRepository, MockUserRepository>();
            services.TryAddSingleton<IOrganizationRepository, MockOrganizationRepository>();
            services.TryAddSingleton<IAzureServiceBusService, MockAzureServiceBusService>();
            services.TryAddSingleton<ILoggerFactory, MockLoggerFactory>();
            services.TryAddSingleton<IIntegrationHandler<object>, MockIntegrationHandler>();

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            var azureServiceBusEventListenerService = serviceProvider.GetService<AzureServiceBusEventListenerService<IIntegrationListenerConfiguration>>();
            var azureServiceBusIntegrationListenerService = serviceProvider.GetService<AzureServiceBusIntegrationListenerService<IIntegrationListenerConfiguration>>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(azureServiceBusEventListenerService);
            Assert.NotNull(azureServiceBusIntegrationListenerService);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_InvalidConfig_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns(null);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object));
        }
    }

    public class MockEventIntegrationPublisher : IEventIntegrationPublisher
    {
        public void PublishEvent(object @event)
        {
            throw new NotImplementedException();
        }
    }

    public class MockIntegrationFilterService : IIntegrationFilterService
    {
        public bool FilterEvent(object @event)
        {
            throw new NotImplementedException();
        }
    }

    public class MockIntegrationConfigurationDetailsCache : IIntegrationConfigurationDetailsCache
    {
        public object GetConfigurationDetails(string routingKey)
        {
            throw new NotImplementedException();
        }
    }

    public class MockUserRepository : IUserRepository
    {
        public object GetUser(string userId)
        {
            throw new NotImplementedException();
        }
    }

    public class MockOrganizationRepository : IOrganizationRepository
    {
        public object GetOrganization(string organizationId)
        {
            throw new NotImplementedException();
        }
    }

    public class MockAzureServiceBusService : IAzureServiceBusService
    {
        public void SendMessage(object message)
        {
            throw new NotImplementedException();
        }
    }

    public class MockLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger<T> CreateLogger<T>() where T : class
        {
            throw new NotImplementedException();
        }
    }

    public class MockIntegrationHandler : IIntegrationHandler<object>
    {
        public void Handle(object @event)
        {
            throw new NotImplementedException();
        }
    }
}
