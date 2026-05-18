using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Enums;
using Bit.Core.Settings;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly IServiceCollection _services;

        public ServiceCollectionExtensionsTests()
        {
            _services = new ServiceCollection();
        }

        [Fact]
        public void AddAzureServiceBusIntegration_EventListenerService_ResolvesILoggerFactory()
        {
            // Arrange
            _services.AddLogging();
            _services.TryAddSingleton<IAzureServiceBusService>(new Mock<IAzureServiceBusService>().Object);
            
            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.Setup(x => x.IntegrationType).Returns(IntegrationType.Webhook);
            mockListenerConfig.Setup(x => x.RoutingKey).Returns("webhook");
            mockListenerConfig.Setup(x => x.EventPrefetchCount).Returns(10);
            mockListenerConfig.Setup(x => x.EventMaxConcurrentCalls).Returns(5);
            mockListenerConfig.Setup(x => x.IntegrationPrefetchCount).Returns(20);
            mockListenerConfig.Setup(x => x.IntegrationMaxConcurrentCalls).Returns(10);

            // Act
            _services.AddAzureServiceBusIntegration<WebhookIntegrationConfigurationDetails, WebhookListenerConfiguration>(
                mockListenerConfig.Object);

            // Assert - Building provider calls GetRequiredService<ILoggerFactory> internally
            var serviceProvider = _services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
        }

        [Fact]
        public void AddAzureServiceBusIntegration_IntegrationListenerService_ResolvesILoggerFactory()
        {
            // Arrange
            _services.AddLogging();
            _services.TryAddSingleton<IAzureServiceBusService>(new Mock<IAzureServiceBusService>().Object);
            
            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.Setup(x => x.IntegrationType).Returns(IntegrationType.Webhook);
            mockListenerConfig.Setup(x => x.RoutingKey).Returns("webhook");
            mockListenerConfig.Setup(x => x.EventPrefetchCount).Returns(10);
            mockListenerConfig.Setup(x => x.EventMaxConcurrentCalls).Returns(5);
            mockListenerConfig.Setup(x => x.IntegrationPrefetchCount).Returns(20);
            mockListenerConfig.Setup(x => x.IntegrationMaxConcurrentCalls).Returns(10);

            // Act
            _services.AddAzureServiceBusIntegration<WebhookIntegrationConfigurationDetails, WebhookListenerConfiguration>(
                mockListenerConfig.Object);

            // Assert - Building provider calls GetRequiredService<ILoggerFactory> internally  
            var serviceProvider = _services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
        }

        [Fact]
        public void AddEventIntegrationServices_ResolvesIIntegrationConfigurationDetailsCache()
        {
            // Arrange
            var globalSettings = new GlobalSettings();

            // Act
            _services.AddEventIntegrationServices(globalSettings);

            // Assert - Building provider calls GetRequiredService<IIntegrationConfigurationDetailsCache> internally
            var serviceProvider = _services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IIntegrationConfigurationDetailsCache>());
        }

        [Fact]
        public void AddAzureServiceBusIntegration_MissingILoggerFactory_ThrowsOnResolution()
        {
            // Arrange
            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.Setup(x => x.IntegrationType).Returns(IntegrationType.Webhook);
            mockListenerConfig.Setup(x => x.RoutingKey).Returns("webhook");

            // Act
            _services.AddAzureServiceBusIntegration<WebhookIntegrationConfigurationDetails, WebhookListenerConfiguration>(
                mockListenerConfig.Object);

            // Assert - GetRequiredService<ILoggerFactory> will throw when no ILoggerFactory registered
            var serviceProvider = _services.BuildServiceProvider();
            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<ILoggerFactory>());
        }
    }
}
