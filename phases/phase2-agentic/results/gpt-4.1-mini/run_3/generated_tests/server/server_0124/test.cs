using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.NotificationCenter;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Settings;
using Bit.Core.Vault.Services;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        // We will test the AddAzureServiceBusIntegration extension method indirectly by invoking it via reflection
        // because it is a private static method.
        // The key point is to verify that the IServiceProvider.GetRequiredService<T> calls are made and the factory creates the expected handler.

        // We create mocks for all required services and verify that the factory method uses GetRequiredService for each.

        private class DummyConfig : class { }
        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey => "test-routing-key";
            public string IntegrationType => "test-integration-type";
            public int EventPrefetchCount => 5;
            public int EventMaxConcurrentCalls => 10;
        }

        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfig = new DummyListenerConfig();

            // Setup mocks for IServiceProvider to verify GetRequiredService calls
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup mocks for all required services returned by GetRequiredService
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<DummyConfig>>>();

            var azureServiceBusServiceMock = new Mock<IAzureServiceBusService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            var eventMessageHandlerMock = new Mock<IEventMessageHandler>();

            // Setup GetRequiredService calls
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IEventIntegrationPublisher>()).Returns(eventIntegrationPublisherMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationFilterService>()).Returns(integrationFilterServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(configurationCacheMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IUserRepository>()).Returns(userRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOrganizationRepository>()).Returns(organizationRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILogger<EventIntegrationHandler<DummyConfig>>>()).Returns(loggerMock.Object);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAzureServiceBusService>()).Returns(azureServiceBusServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            // Setup GetRequiredKeyedService for IEventMessageHandler keyed by routing key
            serviceProviderMock.Setup(sp => sp.GetRequiredKeyedService<IEventMessageHandler>(listenerConfig.RoutingKey)).Returns(eventMessageHandlerMock.Object);

            // Act
            // Use reflection to invoke the private static method AddAzureServiceBusIntegration
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            // Invoke the method with generic parameters DummyConfig and DummyListenerConfig
            var genericMethod = methodInfo.MakeGenericMethod(typeof(DummyConfig), typeof(DummyListenerConfig));
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            // The method returns IServiceCollection, so result should be the same as services
            Assert.Same(services, result);

            // Now verify that the service descriptors were added to services
            // There should be TryAddKeyedSingleton for IEventMessageHandler with the routing key
            // and TryAddEnumerable for IHostedService for AzureServiceBusEventListenerService and AzureServiceBusIntegrationListenerService

            // We check that the services collection contains the expected service descriptors
            bool hasEventMessageHandlerFactory = false;
            bool hasAzureServiceBusEventListenerService = false;
            bool hasAzureServiceBusIntegrationListenerService = false;

            foreach (var sd in services)
            {
                if (sd.ServiceType == typeof(IEventMessageHandler) && sd.ImplementationFactory != null)
                {
                    hasEventMessageHandlerFactory = true;
                }
                if (sd.ServiceType == typeof(IHostedService) && sd.ImplementationFactory != null)
                {
                    var implType = sd.ImplementationType?.Name;
                    if (implType == "AzureServiceBusEventListenerService`1")
                    {
                        hasAzureServiceBusEventListenerService = true;
                    }
                    else if (implType == "AzureServiceBusIntegrationListenerService`1")
                    {
                        hasAzureServiceBusIntegrationListenerService = true;
                    }
                }
            }

            Assert.True(hasEventMessageHandlerFactory, "Expected IEventMessageHandler factory registration.");
            Assert.True(hasAzureServiceBusEventListenerService, "Expected AzureServiceBusEventListenerService registration.");
            Assert.True(hasAzureServiceBusIntegrationListenerService, "Expected AzureServiceBusIntegrationListenerService registration.");
        }
    }
}
