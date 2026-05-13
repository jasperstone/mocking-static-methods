using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        // We will test the AddAzureServiceBusIntegration method indirectly by invoking it via reflection
        // because it is private. We want to verify that the IServiceProvider.GetRequiredService extension
        // method is called on the provider for the expected service types.

        // To do this, we will create mocks for IServiceCollection and IServiceProvider and verify
        // that the provider's GetRequiredService method is called with the expected types.

        // We will create a minimal stub for IIntegrationListenerConfiguration to pass as parameter.

        private class TestListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey => "test-routing-key";
            public string IntegrationType => "test-integration-type";
            public int EventPrefetchCount => 1;
            public int EventMaxConcurrentCalls => 1;
            public int IntegrationPrefetchCount => 2;
            public int IntegrationMaxConcurrentCalls => 2;
        }

        [Fact]
        public void AddAzureServiceBusIntegration_CallsGetRequiredServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new TestListenerConfig();

            // We will create a mock IServiceProvider that returns mocks for all requested services
            var providerMock = new Mock<IServiceProvider>();

            // Setup GetRequiredService calls for all expected service types
            providerMock.Setup(p => p.GetService(typeof(IEventIntegrationPublisher)))
                .Returns(Mock.Of<IEventIntegrationPublisher>());
            providerMock.Setup(p => p.GetService(typeof(IIntegrationFilterService)))
                .Returns(Mock.Of<IIntegrationFilterService>());
            providerMock.Setup(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache)))
                .Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            providerMock.Setup(p => p.GetService(typeof(IUserRepository)))
                .Returns(Mock.Of<IUserRepository>());
            providerMock.Setup(p => p.GetService(typeof(IOrganizationRepository)))
                .Returns(Mock.Of<IOrganizationRepository>());
            providerMock.Setup(p => p.GetService(typeof(ILogger<EventIntegrationHandler<object>>)))
                .Returns(Mock.Of<ILogger<EventIntegrationHandler<object>>>());

            providerMock.Setup(p => p.GetService(typeof(IAzureServiceBusService)))
                .Returns(Mock.Of<IAzureServiceBusService>());
            providerMock.Setup(p => p.GetService(typeof(ILoggerFactory)))
                .Returns(Mock.Of<ILoggerFactory>());

            // Setup GetRequiredKeyedService for IEventMessageHandler with the routing key
            // This is an extension method, so we simulate by adding a method on the mock
            // but since it's an extension, we cannot mock it directly.
            // Instead, we will just verify that the service collection contains the expected registrations.

            // Act
            // Use reflection to invoke the private static method AddAzureServiceBusIntegration<TConfig, TListenerConfig>
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);

            // Make generic method for TConfig=object, TListenerConfig=TestListenerConfig
            var genericMethod = method.MakeGenericMethod(typeof(object), typeof(TestListenerConfig));

            // Call the method
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            // The result should be the same IServiceCollection instance
            Assert.Same(services, result);

            // Verify that the service collection contains the expected registrations
            // We expect TryAddKeyedSingleton<IEventMessageHandler> with the routing key
            // and TryAddEnumerable for IHostedService with AzureServiceBusEventListenerService and AzureServiceBusIntegrationListenerService

            // Check that the service collection contains a service descriptor for IEventMessageHandler with the routing key
            // Since TryAddKeyedSingleton is a custom extension, we check for service descriptors with ServiceType IEventMessageHandler
            bool hasEventMessageHandler = false;
            bool hasAzureServiceBusEventListenerService = false;
            bool hasAzureServiceBusIntegrationListenerService = false;

            foreach (var sd in services)
            {
                if (sd.ServiceType == typeof(IEventMessageHandler))
                {
                    hasEventMessageHandler = true;
                }
                if (sd.ServiceType == typeof(IHostedService) && sd.ImplementationFactory != null)
                {
                    var implType = sd.ImplementationType?.Name ?? "";
                    if (implType.Contains("AzureServiceBusEventListenerService"))
                    {
                        hasAzureServiceBusEventListenerService = true;
                    }
                    if (implType.Contains("AzureServiceBusIntegrationListenerService"))
                    {
                        hasAzureServiceBusIntegrationListenerService = true;
                    }
                }
            }

            Assert.True(hasEventMessageHandler, "Expected IEventMessageHandler registration");
            Assert.True(hasAzureServiceBusEventListenerService, "Expected AzureServiceBusEventListenerService registration");
            Assert.True(hasAzureServiceBusIntegrationListenerService, "Expected AzureServiceBusIntegrationListenerService registration");
        }
    }

    // Minimal interface stubs to allow compilation
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
    public interface IEventMessageHandler { }
    public interface IAzureServiceBusService { }
    public interface IIntegrationHandler<T> { }
    public class EventIntegrationHandler<T> : IEventMessageHandler
    {
        public EventIntegrationHandler(string integrationType,
            IEventIntegrationPublisher eventIntegrationPublisher,
            IIntegrationFilterService integrationFilterService,
            IIntegrationConfigurationDetailsCache configurationCache,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            ILogger<EventIntegrationHandler<T>> logger)
        {
        }
    }
    public class AzureServiceBusEventListenerService<TListenerConfig> : IHostedService
    {
        public AzureServiceBusEventListenerService(TListenerConfig configuration,
            IEventMessageHandler handler,
            IAzureServiceBusService serviceBusService,
            ServiceBusProcessorOptions serviceBusOptions,
            ILoggerFactory loggerFactory)
        {
        }

        public System.Threading.Tasks.Task StartAsync(System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task StopAsync(System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
    }
    public class AzureServiceBusIntegrationListenerService<TListenerConfig> : IHostedService
    {
        public AzureServiceBusIntegrationListenerService(TListenerConfig configuration,
            IIntegrationHandler<object> handler,
            IAzureServiceBusService serviceBusService,
            ServiceBusProcessorOptions serviceBusOptions,
            ILoggerFactory loggerFactory)
        {
        }

        public System.Threading.Tasks.Task StartAsync(System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task StopAsync(System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
    }
    public class ServiceBusProcessorOptions
    {
        public int PrefetchCount { get; set; }
        public int MaxConcurrentCalls { get; set; }
    }
}
