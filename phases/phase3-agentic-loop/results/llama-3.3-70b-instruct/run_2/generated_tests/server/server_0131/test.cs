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
            services.AddAzureServiceBusIntegration<object, MockListenerConfig>(listenerConfiguration.Object);
            // Act
            var serviceProvider = services.BuildServiceProvider();
            // Assert
            Assert.NotNull(serviceProvider.GetService(typeof(IEventMessageHandler)));
        }
        [Fact]
        public void AddAzureServiceBusIntegration_InvalidConfig_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns(null);
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddAzureServiceBusIntegration<object, MockListenerConfig>(listenerConfiguration.Object));
        }
    }
    public class MockListenerConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey { get; set; }
        public string IntegrationType { get; set; }
        public int EventPrefetchCount { get; set; }
        public int EventMaxConcurrentCalls { get; set; }
        public int IntegrationPrefetchCount { get; set; }
        public int IntegrationMaxConcurrentCalls { get; set; }
    }
}
