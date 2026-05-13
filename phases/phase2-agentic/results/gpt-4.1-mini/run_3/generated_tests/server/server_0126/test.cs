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
        // We want to test the AddAzureServiceBusIntegration extension method indirectly
        // because it is private. We will test the behavior of the service registrations
        // that rely on the IServiceProvider.GetRequiredService calls.

        // To do this, we create a minimal test that registers the dependencies and then
        // verifies that the services can be resolved, which exercises the GetRequiredService calls.

        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; } = "test-routing-key";
            public string IntegrationType { get; set; } = "test-integration-type";
            public int EventPrefetchCount { get; set; } = 1;
            public int EventMaxConcurrentCalls { get; set; } = 1;
            public int IntegrationPrefetchCount { get; set; } = 1;
            public int IntegrationMaxConcurrentCalls { get; set; } = 1;
        }

        private class DummyConfig { }

        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServices_And_ResolvesRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new DummyListenerConfig();

            // Setup mocks for all required services that GetRequiredService will request
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<DummyConfig>>>();

            var eventMessageHandlerMock = new Mock<IEventMessageHandler>();
            var azureServiceBusServiceMock = new Mock<IAzureServiceBusService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var integrationHandlerMock = new Mock<IIntegrationHandler<DummyConfig>>();

            // Register mocks in the service collection
            services.AddSingleton(eventIntegrationPublisherMock.Object);
            services.AddSingleton(integrationFilterServiceMock.Object);
            services.AddSingleton(configurationCacheMock.Object);
            services.AddSingleton(userRepositoryMock.Object);
            services.AddSingleton(organizationRepositoryMock.Object);
            services.AddSingleton(loggerMock.Object);
            services.AddSingleton(azureServiceBusServiceMock.Object);
            services.AddSingleton(loggerFactoryMock.Object);
            services.AddSingleton(integrationHandlerMock.Object);

            // We need to register the TryAddKeyedSingleton and TryAddEnumerable extension methods
            // but since they are not standard, we will simulate the AddAzureServiceBusIntegration method
            // by calling it via reflection or by replicating the registration here.

            // Instead, we will call the private AddAzureServiceBusIntegration method via reflection.

            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            // Invoke the method with generic parameters DummyConfig and DummyListenerConfig
            var genericMethod = methodInfo.MakeGenericMethod(typeof(DummyConfig), typeof(DummyListenerConfig));
            genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Act & Assert
            // Resolve IEventMessageHandler keyed by routing key - simulate by resolving all IEventMessageHandler
            var handlers = provider.GetServices<IEventMessageHandler>();
            Assert.Contains(handlers, h => h != null);

            // Resolve IHostedService implementations registered by AddAzureServiceBusIntegration
            var hostedServices = provider.GetServices<IHostedService>();
            Assert.Contains(hostedServices, hs => hs != null);

            // Also verify that the EventIntegrationHandler<DummyConfig> can be resolved indirectly
            var eventHandler = provider.GetService<IEventMessageHandler>();
            Assert.NotNull(eventHandler);
        }
    }
}
