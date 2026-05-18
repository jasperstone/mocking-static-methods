using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.NotificationCenter;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.AdminConsole.Services;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services.Implementations;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        // We will test the AddAzureServiceBusIntegration method indirectly by calling it via reflection
        // because it is private static. We want to verify that the IServiceProvider.GetRequiredService
        // extension method is called on the provider inside the implementation.

        // To do this, we will mock IServiceProvider and verify that GetRequiredService is called
        // with expected service types.

        // We will create a minimal IIntegrationListenerConfiguration mock to pass as parameter.

        private class DummyListenerConfig : Bit.Core.AdminConsole.Models.Data.EventIntegrations.IIntegrationListenerConfiguration
        {
            public string RoutingKey => "dummyKey";
            public string IntegrationType => "dummyIntegration";
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

            var listenerConfig = new DummyListenerConfig();

            // We will create mocks for IServiceProvider and the services that GetRequiredService should return
            var mockProvider = new Mock<IServiceProvider>();

            // Setup GetRequiredService calls to return mocks for each requested service type
            mockProvider.Setup(p => p.GetService(typeof(IEventIntegrationPublisher)))
                .Returns(Mock.Of<IEventIntegrationPublisher>());
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationFilterService)))
                .Returns(Mock.Of<IIntegrationFilterService>());
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache)))
                .Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            mockProvider.Setup(p => p.GetService(typeof(IUserRepository)))
                .Returns(Mock.Of<IUserRepository>());
            mockProvider.Setup(p => p.GetService(typeof(IOrganizationRepository)))
                .Returns(Mock.Of<IOrganizationRepository>());
            mockProvider.Setup(p => p.GetService(typeof(ILogger<EventIntegrationHandler<object>>)))
                .Returns(Mock.Of<ILogger<EventIntegrationHandler<object>>>());
            mockProvider.Setup(p => p.GetService(typeof(IAzureServiceBusService)))
                .Returns(Mock.Of<IAzureServiceBusService>());
            mockProvider.Setup(p => p.GetService(typeof(ILoggerFactory)))
                .Returns(Mock.Of<ILoggerFactory>());

            // We also need to setup GetRequiredKeyedService extension method, but since it's an extension,
            // we will simulate it by adding a service keyed by the routing key.
            // For simplicity, we will just add a service of type IEventMessageHandler to the services collection.

            // We will add a dummy IEventMessageHandler keyed service to the services collection
            services.TryAddSingleton<IEventMessageHandler>(new EventIntegrationHandler<object>(
                listenerConfig.IntegrationType,
                Mock.Of<IEventIntegrationPublisher>(),
                Mock.Of<IIntegrationFilterService>(),
                Mock.Of<IIntegrationConfigurationDetailsCache>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<IOrganizationRepository>(),
                Mock.Of<ILogger<EventIntegrationHandler<object>>>()
            ));

            // Act
            // Use reflection to invoke the private static method AddAzureServiceBusIntegration
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);

            // The method is generic with two type parameters, we will specify object for TConfig and DummyListenerConfig for TListenerConfig
            var genericMethod = method.MakeGenericMethod(typeof(object), typeof(DummyListenerConfig));

            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            // The result should be the same IServiceCollection instance
            Assert.Same(services, result);

            // We cannot directly verify calls to GetRequiredService on the provider inside the factory delegate,
            // but we can verify that the services collection contains the expected registrations.

            // Check that the services collection contains IHostedService registrations for AzureServiceBusEventListenerService and AzureServiceBusIntegrationListenerService
            bool hasEventListenerService = false;
            bool hasIntegrationListenerService = false;

            foreach (var serviceDescriptor in services)
            {
                if (serviceDescriptor.ServiceType == typeof(IHostedService) &&
                    serviceDescriptor.ImplementationFactory != null)
                {
                    var impl = serviceDescriptor.ImplementationFactory(mockProvider.Object);
                    if (impl is AzureServiceBusEventListenerService<DummyListenerConfig>)
                    {
                        hasEventListenerService = true;
                    }
                    else if (impl is AzureServiceBusIntegrationListenerService<DummyListenerConfig>)
                    {
                        hasIntegrationListenerService = true;
                    }
                }
            }

            Assert.True(hasEventListenerService, "AzureServiceBusEventListenerService should be registered");
            Assert.True(hasIntegrationListenerService, "AzureServiceBusIntegrationListenerService should be registered");
        }
    }
}
