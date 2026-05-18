using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ReturnsSameServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a minimal implementation of IIntegrationListenerConfiguration
            var listenerConfig = new TestIntegrationListenerConfiguration();

            // Act
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(methodInfo);

            var genericMethod = methodInfo.MakeGenericMethod(typeof(object), typeof(IIntegrationListenerConfiguration));

            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.Same(services, result);
        }

        private class TestIntegrationListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey => "test-routing-key";
            public string IntegrationType => "test-integration-type";
            public int EventPrefetchCount => 1;
            public int EventMaxConcurrentCalls => 1;
            public int IntegrationPrefetchCount => 1;
            public int IntegrationMaxConcurrentCalls => 1;
        }
    }
}
