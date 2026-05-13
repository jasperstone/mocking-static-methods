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
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        // We will test the AddAzureServiceBusIntegration extension method indirectly by verifying that
        // the IServiceCollection has the expected registrations and that the factory calls GetRequiredService on IServiceProvider.

        // Since the method is private, we will use reflection to invoke it.

        private static IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }

        private static IServiceCollection AddAzureServiceBusIntegration<TConfig, TListenerConfig>(IServiceCollection services, TListenerConfig listenerConfiguration)
            where TConfig : class
            where TListenerConfig : IIntegrationListenerConfiguration
        {
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method == null)
                throw new InvalidOperationException("AddAzureServiceBusIntegration method not found");

            return (IServiceCollection)method.MakeGenericMethod(typeof(TConfig), typeof(TListenerConfig)).Invoke(null, new object[] { services, listenerConfiguration });
        }

        // Define a minimal IIntegrationListenerConfiguration implementation for testing
        private class TestListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; } = "test-routing-key";
            public string IntegrationType { get; set; } = "test-integration-type";
            public int EventPrefetchCount { get; set; } = 5;
            public int EventMaxConcurrentCalls { get; set; } = 10;
        }

        // Define a dummy TConfig class
        private class DummyConfig { }

        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = CreateServiceCollection();
            var listenerConfig = new TestListenerConfig();

            // Setup mocks for the services that will be requested by the factory
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup mocks for all required services to be returned by GetRequiredService
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<DummyConfig>>>();

            // Setup GetRequiredService calls for EventIntegrationHandler factory
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher))).Returns(eventIntegrationPublisherMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService))).Returns(integrationFilterServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(configurationCacheMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository))).Returns(organizationRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>))).Returns(loggerMock.Object);

            // Setup mocks for the second and third IHostedService registrations
            var eventMessageHandlerMock = new Mock<IEventMessageHandler>();
            var azureServiceBusServiceMock = new Mock<IAzureServiceBusService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var integrationHandlerMock = new Mock<IIntegrationHandler<DummyConfig>>();

            // Setup GetRequiredService calls for AzureServiceBusEventListenerService factory
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventMessageHandler))).Returns(eventMessageHandlerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAzureServiceBusService))).Returns(azureServiceBusServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Setup GetRequiredService calls for AzureServiceBusIntegrationListenerService factory
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationHandler<DummyConfig>))).Returns(integrationHandlerMock.Object);

            // Act
            AddAzureServiceBusIntegration<DummyConfig, TestListenerConfig>(services, listenerConfig);

            // Build the service provider to test the factories
            var builtProvider = services.BuildServiceProvider();

            // We will test the first factory by invoking it manually and verifying that GetRequiredService was called on the service provider mock
            var serviceDescriptor = Assert.Single(services, sd => sd.ServiceType == typeof(IEventMessageHandler) && sd.ImplementationFactory != null);
            var factory = serviceDescriptor.ImplementationFactory;

            // We need to create a mock IServiceProvider that tracks calls to GetRequiredService
            var spMock = new Mock<IServiceProvider>();

            // Setup GetRequiredService to return mocks for all required services
            spMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher))).Returns(eventIntegrationPublisherMock.Object).Verifiable();
            spMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService))).Returns(integrationFilterServiceMock.Object).Verifiable();
            spMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(configurationCacheMock.Object).Verifiable();
            spMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepositoryMock.Object).Verifiable();
            spMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository))).Returns(organizationRepositoryMock.Object).Verifiable();
            spMock.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>))).Returns(loggerMock.Object).Verifiable();

            // Invoke the factory
            var handler = factory(spMock.Object);

            // Assert that the factory returned a non-null handler
            Assert.NotNull(handler);

            // Verify that GetService was called for all required services
            spMock.Verify(sp => sp.GetService(typeof(IEventIntegrationPublisher)), Times.Once);
            spMock.Verify(sp => sp.GetService(typeof(IIntegrationFilterService)), Times.Once);
            spMock.Verify(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.Once);
            spMock.Verify(sp => sp.GetService(typeof(IUserRepository)), Times.Once);
            spMock.Verify(sp => sp.GetService(typeof(IOrganizationRepository)), Times.Once);
            spMock.Verify(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>)), Times.Once);
        }
    }
}
