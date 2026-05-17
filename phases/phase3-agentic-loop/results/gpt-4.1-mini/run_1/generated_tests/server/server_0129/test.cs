using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private class TestListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
            public int IntegrationPrefetchCount { get; set; }
            public int IntegrationMaxConcurrentCalls { get; set; }
        }

        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new TestListenerConfig
            {
                RoutingKey = "testRoutingKey",
                IntegrationType = "testIntegrationType",
                EventPrefetchCount = 5,
                EventMaxConcurrentCalls = 10,
                IntegrationPrefetchCount = 3,
                IntegrationMaxConcurrentCalls = 7
            };

            // Act
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(methodInfo);

            var genericMethod = methodInfo.MakeGenericMethod(typeof(object), typeof(TestListenerConfig));
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IServiceCollection>(result);

            // Check that services contain registrations for IEventMessageHandler and IHostedService
            Assert.Contains(services, sd => sd.ServiceType == typeof(IEventMessageHandler));
            Assert.Contains(services, sd => sd.ServiceType == typeof(IHostedService));
        }
    }

    // Minimal interface definitions to allow compilation of the test
    public interface IIntegrationListenerConfiguration
    {
        string RoutingKey { get; }
        string IntegrationType { get; }
        int EventPrefetchCount { get; }
        int EventMaxConcurrentCalls { get; }
        int IntegrationPrefetchCount { get; }
        int IntegrationMaxConcurrentCalls { get; }
    }

    public interface IEventMessageHandler { }
}
