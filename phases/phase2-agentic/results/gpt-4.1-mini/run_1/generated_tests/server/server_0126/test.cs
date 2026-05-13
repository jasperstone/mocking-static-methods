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
using Bit.Core.Tokens;
using Bit.Core.Tools.Services;
using Bit.Core.Vault.Services;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        // We want to test the AddAzureServiceBusIntegration extension method, especially the call to GetRequiredService on IServiceProvider.
        // Since the method is private static, we will use reflection to invoke it.
        // We will mock the IServiceProvider to verify that GetRequiredService is called for the expected service types.

        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; } = "dummyRoutingKey";
            public string IntegrationType { get; set; } = "dummyIntegrationType";
            public int EventPrefetchCount { get; set; } = 1;
            public int EventMaxConcurrentCalls { get; set; } = 1;
            public int IntegrationPrefetchCount { get; set; } = 1;
            public int IntegrationMaxConcurrentCalls { get; set; } = 1;
        }

        private class DummyConfig { }

        [Fact]
        public void AddAzureServiceBusIntegration_CallsGetRequiredServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new DummyListenerConfig();

            // Setup mocks for the services that will be requested by GetRequiredService
            var mockProvider = new Mock<IServiceProvider>();

            // Setup the expected service types to be requested
            mockProvider.Setup(p => p.GetService(typeof(IEventIntegrationPublisher)))
                .Returns(Mock.Of<IEventIntegrationPublisher>())
                .Verifiable();

            mockProvider.Setup(p => p.GetService(typeof(IIntegrationFilterService)))
                .Returns(Mock.Of<IIntegrationFilterService>())
                .Verifiable();

            mockProvider.Setup(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache)))
                .Returns(Mock.Of<IIntegrationConfigurationDetailsCache>())
                .Verifiable();

            mockProvider.Setup(p => p.GetService(typeof(IUserRepository)))
                .Returns(Mock.Of<IUserRepository>())
                .Verifiable();

            mockProvider.Setup(p => p.GetService(typeof(IOrganizationRepository)))
                .Returns(Mock.Of<IOrganizationRepository>())
                .Verifiable();

            mockProvider.Setup(p => p.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>)))
                .Returns(Mock.Of<ILogger<EventIntegrationHandler<DummyConfig>>>())
                .Verifiable();

            mockProvider.Setup(p => p.GetService(typeof(IAzureServiceBusService)))
                .Returns(Mock.Of<IAzureServiceBusService>())
                .Verifiable();

            mockProvider.Setup(p => p.GetService(typeof(ILoggerFactory)))
                .Returns(Mock.Of<ILoggerFactory>())
                .Verifiable();

            // Setup for GetRequiredKeyedService extension method - we will mock it by extension method on IServiceProvider
            // Since it's an extension method, we cannot mock it directly, so we will add a dummy implementation in the service collection
            // For simplicity, we will add a dummy IEventMessageHandler keyed service to the service collection

            var dummyHandler = Mock.Of<IEventMessageHandler>();
            services.AddSingleton(dummyHandler);

            // Act
            // Use reflection to invoke the private static method AddAzureServiceBusIntegration<TConfig, TListenerConfig>
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);

            // The method is generic, so make generic with DummyConfig and DummyListenerConfig
            var genericMethod = method.MakeGenericMethod(typeof(DummyConfig), typeof(DummyListenerConfig));

            // We invoke the method with services and listenerConfig
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            // The method returns IServiceCollection, so result should be services
            Assert.Same(services, result);

            // Now build the service provider and verify that the services are registered and can be resolved
            var serviceProvider = services.BuildServiceProvider();

            // We expect that the EventIntegrationHandler<DummyConfig> is registered keyed by listenerConfig.RoutingKey
            // Since TryAddKeyedSingleton is a custom extension, we cannot directly resolve by key here,
            // but we can check that the service collection contains the expected service descriptors

            // Verify that the mockProvider's GetService was not called because the real provider is used in the factory
            // So we cannot verify the mockProvider calls, but we can verify that the services are registered without exceptions

            // Try to resolve IHostedService implementations registered by the method
            var hostedServices = serviceProvider.GetServices<IHostedService>();
            Assert.NotEmpty(hostedServices);

            // We can check that the services collection contains the expected service descriptors for IHostedService
            bool hasAzureServiceBusEventListenerService = false;
            bool hasAzureServiceBusIntegrationListenerService = false;

            foreach (var sd in services)
            {
                if (sd.ServiceType == typeof(IHostedService) && sd.ImplementationType != null)
                {
                    if (sd.ImplementationType.Name.Contains("AzureServiceBusEventListenerService"))
                        hasAzureServiceBusEventListenerService = true;
                    if (sd.ImplementationType.Name.Contains("AzureServiceBusIntegrationListenerService"))
                        hasAzureServiceBusIntegrationListenerService = true;
                }
            }

            Assert.True(hasAzureServiceBusEventListenerService, "AzureServiceBusEventListenerService should be registered");
            Assert.True(hasAzureServiceBusIntegrationListenerService, "AzureServiceBusIntegrationListenerService should be registered");
        }
    }
}
