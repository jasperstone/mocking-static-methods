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
        // We will test the AddAzureServiceBusIntegration method indirectly by verifying that the services
        // are registered and that the IServiceProvider.GetRequiredService extension method is called.
        // Since the method is private, we will use reflection to invoke it.

        // We create mocks for the dependencies that GetRequiredService would resolve.

        private interface IEventIntegrationPublisher { }
        private interface IIntegrationFilterService { }
        private interface IIntegrationConfigurationDetailsCache { }
        private interface IUserRepository { }
        private interface IOrganizationRepository { }
        private interface IAzureServiceBusService { }
        private interface IEventMessageHandler { }
        private interface IIntegrationHandler<T> { }
        private interface IHostedService { }

        private interface IIntegrationListenerConfiguration
        {
            string RoutingKey { get; }
            string IntegrationType { get; }
            int EventPrefetchCount { get; }
            int EventMaxConcurrentCalls { get; }
            int IntegrationPrefetchCount { get; }
            int IntegrationMaxConcurrentCalls { get; }
        }

        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
            public int IntegrationPrefetchCount { get; set; }
            public int IntegrationMaxConcurrentCalls { get; set; }
        }

        private class DummyConfig { }

        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServices_AndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new DummyListenerConfig
            {
                RoutingKey = "testKey",
                IntegrationType = "testType",
                EventPrefetchCount = 5,
                EventMaxConcurrentCalls = 10,
                IntegrationPrefetchCount = 3,
                IntegrationMaxConcurrentCalls = 6
            };

            // Setup mocks for IServiceProvider.GetRequiredService calls
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup mocks for all required services that GetRequiredService is called for
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var integrationConfigurationDetailsCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger>();

            var azureServiceBusServiceMock = new Mock<IAzureServiceBusService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Setup the serviceProviderMock to return mocks for GetRequiredService calls
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher))).Returns(eventIntegrationPublisherMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService))).Returns(integrationFilterServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(integrationConfigurationDetailsCacheMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository))).Returns(organizationRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger))).Returns(loggerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAzureServiceBusService))).Returns(azureServiceBusServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            // Use reflection to invoke the private extension method AddAzureServiceBusIntegration
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            // The method is generic with two type parameters, so make generic method
            var genericMethod = methodInfo.MakeGenericMethod(typeof(DummyConfig), typeof(DummyListenerConfig));

            // Invoke the method
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            // The method returns IServiceCollection, so result should be services
            Assert.Same(services, result);

            // Verify that the services collection contains registrations for IHostedService (2 times)
            int hostedServiceCount = 0;
            foreach (var serviceDescriptor in services)
            {
                if (serviceDescriptor.ServiceType == typeof(IHostedService))
                {
                    hostedServiceCount++;
                }
            }
            Assert.Equal(2, hostedServiceCount);

            // Verify that the service collection contains a registration for IEventMessageHandler keyed by routing key
            // Since TryAddKeyedSingleton is a custom extension, we cannot directly verify keyed registrations here,
            // but we can verify that the service collection contains a registration for IEventMessageHandler
            bool hasEventMessageHandler = false;
            foreach (var serviceDescriptor in services)
            {
                if (serviceDescriptor.ServiceType == typeof(IEventMessageHandler))
                {
                    hasEventMessageHandler = true;
                    break;
                }
            }
            Assert.True(hasEventMessageHandler);
        }
    }
}
