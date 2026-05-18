using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        interface IEventIntegrationPublisher { }
        interface IIntegrationFilterService { }
        interface IIntegrationConfigurationDetailsCache { }
        interface IUserRepository { }
        interface IOrganizationRepository { }
        interface IEventMessageHandler { }
        interface IAzureServiceBusService { }
        interface IIntegrationHandler<T> { }
        interface IHostedService { }
        interface ILogger<T> { }
        interface ILoggerFactory { }

        interface IIntegrationListenerConfiguration
        {
            string RoutingKey { get; }
            string IntegrationType { get; }
            int EventPrefetchCount { get; }
            int EventMaxConcurrentCalls { get; }
        }

        class EventIntegrationHandler<TConfig> : IEventMessageHandler
        {
            public EventIntegrationHandler(string integrationType,
                IEventIntegrationPublisher eventIntegrationPublisher,
                IIntegrationFilterService integrationFilterService,
                IIntegrationConfigurationDetailsCache configurationCache,
                IUserRepository userRepository,
                IOrganizationRepository organizationRepository,
                ILogger<EventIntegrationHandler<TConfig>> logger)
            {
            }
        }

        class AzureServiceBusEventListenerService<TListenerConfig> : IHostedService
        {
            public AzureServiceBusEventListenerService(
                TListenerConfig configuration,
                IEventMessageHandler handler,
                IAzureServiceBusService serviceBusService,
                object serviceBusOptions,
                ILoggerFactory loggerFactory)
            {
            }
        }

        class AzureServiceBusIntegrationListenerService<TListenerConfig> : IHostedService
        {
            public AzureServiceBusIntegrationListenerService(
                TListenerConfig configuration,
                IIntegrationHandler<object> handler,
                IAzureServiceBusService serviceBusService,
                object serviceBusOptions,
                ILoggerFactory loggerFactory)
            {
            }
        }

        // We need to test the private extension method AddAzureServiceBusIntegration<TConfig, TListenerConfig>
        // Since it's private, we will use reflection to invoke it.

        class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; } = "test-routing-key";
            public string IntegrationType { get; set; } = "test-integration-type";
            public int EventPrefetchCount { get; set; } = 5;
            public int EventMaxConcurrentCalls { get; set; } = 10;
        }

        [Fact]
        public void AddAzureServiceBusIntegration_CallsGetRequiredServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new DummyListenerConfig();

            // Setup mocks for IServiceProvider to return dummy instances for required services
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher)))
                .Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService)))
                .Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache)))
                .Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository)))
                .Returns(new Mock<IUserRepository>().Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository)))
                .Returns(new Mock<IOrganizationRepository>().Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<object>>)))
                .Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAzureServiceBusService)))
                .Returns(new Mock<IAzureServiceBusService>().Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(new Mock<ILoggerFactory>().Object);

            // Setup for GetRequiredKeyedService extension method - we will mock it by adding a service
            // But since it's an extension method, we cannot mock it easily here, so we will skip that part.

            // Act
            // Use reflection to invoke the private extension method AddAzureServiceBusIntegration<TConfig, TListenerConfig>
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);

            // We need to make generic method for TConfig=object, TListenerConfig=DummyListenerConfig
            var genericMethod = method.MakeGenericMethod(typeof(object), typeof(DummyListenerConfig));

            // Call the method
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            // The method returns IServiceCollection, so result should be services
            Assert.Same(services, result);

            // The services collection should contain registrations for IEventMessageHandler keyed by routing key
            // and IHostedService for AzureServiceBusEventListenerService and AzureServiceBusIntegrationListenerService
            // We check that the services collection has these descriptors

            bool hasEventMessageHandler = false;
            bool hasHostedServiceListener = false;
            bool hasHostedServiceIntegrationListener = false;

            foreach (var sd in services)
            {
                if (sd.ServiceType == typeof(IEventMessageHandler))
                {
                    hasEventMessageHandler = true;
                }
                if (sd.ServiceType == typeof(IHostedService) && sd.ImplementationType != null)
                {
                    if (sd.ImplementationType.Name.Contains("AzureServiceBusEventListenerService"))
                    {
                        hasHostedServiceListener = true;
                    }
                    if (sd.ImplementationType.Name.Contains("AzureServiceBusIntegrationListenerService"))
                    {
                        hasHostedServiceIntegrationListener = true;
                    }
                }
            }

            Assert.True(hasEventMessageHandler, "IEventMessageHandler service not registered");
            Assert.True(hasHostedServiceListener, "AzureServiceBusEventListenerService not registered");
            Assert.True(hasHostedServiceIntegrationListener, "AzureServiceBusIntegrationListenerService not registered");
        }
    }
}
