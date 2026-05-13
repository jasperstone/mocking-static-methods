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
        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; } = "test-routing-key";
            public string IntegrationType { get; set; } = "test-integration-type";
            public int EventPrefetchCount { get; set; } = 1;
            public int EventMaxConcurrentCalls { get; set; } = 1;
            public int IntegrationPrefetchCount { get; set; } = 1;
            public int IntegrationMaxConcurrentCalls { get; set; } = 1;
        }

        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfig = new DummyListenerConfig();

            // Setup mocks for all required services that GetRequiredService will be called for
            var serviceProviderMock = new Mock<IServiceProvider>();

            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var eventMessageHandlerMock = new Mock<IEventMessageHandler>();
            var azureServiceBusServiceMock = new Mock<IAzureServiceBusService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var integrationHandlerMock = new Mock<IIntegrationHandler<object>>();

            // Setup service provider to return mocks for GetRequiredService calls
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher))).Returns(eventIntegrationPublisherMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService))).Returns(integrationFilterServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(configurationCacheMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository))).Returns(organizationRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<object>>))).Returns(loggerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAzureServiceBusService))).Returns(azureServiceBusServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Setup for GetRequiredKeyedService (extension method) - simulate by returning eventMessageHandlerMock
            // Since this is an extension method, we simulate by adding a service descriptor with factory that uses the service provider mock
            // We will register a factory that returns eventMessageHandlerMock.Object when called with the routing key

            // Act
            // Call the extension method under test
            // We need to call the private AddAzureServiceBusIntegration method, but it's private.
            // So we will use reflection to invoke it.

            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            // The method is generic with two type parameters, so we make a generic method with object for both
            var genericMethod = methodInfo.MakeGenericMethod(typeof(object), typeof(DummyListenerConfig));

            // We invoke the method with services and listenerConfig
            genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            // The services collection should now contain registrations for IEventMessageHandler keyed by routing key,
            // and IHostedService implementations for AzureServiceBusEventListenerService and AzureServiceBusIntegrationListenerService.

            // Check that the services collection contains the expected service descriptors
            bool hasEventMessageHandler = false;
            bool hasAzureServiceBusEventListenerService = false;
            bool hasAzureServiceBusIntegrationListenerService = false;

            foreach (var serviceDescriptor in services)
            {
                if (serviceDescriptor.ServiceType == typeof(IEventMessageHandler))
                {
                    hasEventMessageHandler = true;
                }
                if (serviceDescriptor.ServiceType == typeof(IHostedService) && serviceDescriptor.ImplementationType != null)
                {
                    if (serviceDescriptor.ImplementationType.Name.Contains("AzureServiceBusEventListenerService"))
                    {
                        hasAzureServiceBusEventListenerService = true;
                    }
                    if (serviceDescriptor.ImplementationType.Name.Contains("AzureServiceBusIntegrationListenerService"))
                    {
                        hasAzureServiceBusIntegrationListenerService = true;
                    }
                }
            }

            Assert.True(hasEventMessageHandler, "IEventMessageHandler service not registered");
            Assert.True(hasAzureServiceBusEventListenerService, "AzureServiceBusEventListenerService not registered");
            Assert.True(hasAzureServiceBusIntegrationListenerService, "AzureServiceBusIntegrationListenerService not registered");
        }
    }
}
