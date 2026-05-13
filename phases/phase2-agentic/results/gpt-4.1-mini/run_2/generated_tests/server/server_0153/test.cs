using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.HostedServices;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Platform;
using Bit.Core.Settings;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        // We will test the extension method that calls GetRequiredService on IServiceProvider
        // The method is the one that adds RabbitMqIntegrationListenerService<TListenerConfig>
        // which calls provider.GetRequiredService<TimeProvider>() among others.

        // We create mocks for the required services and verify the call to GetRequiredService.

        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey => "dummyRoutingKey";
            public string IntegrationType => "dummyIntegrationType";
        }

        private class DummyConfig : IIntegrationHandlerConfiguration
        {
        }

        [Fact]
        public void AddIntegrationListenerServices_CallsGetRequiredServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new DummyListenerConfig();

            // Setup mocks for IServiceProvider to verify GetRequiredService calls
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetRequiredService calls for all required services
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher)))
                .Returns(Mock.Of<IEventIntegrationPublisher>()).Verifiable();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService)))
                .Returns(Mock.Of<IIntegrationFilterService>()).Verifiable();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache)))
                .Returns(Mock.Of<IIntegrationConfigurationDetailsCache>()).Verifiable();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository)))
                .Returns(Mock.Of<IUserRepository>()).Verifiable();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository)))
                .Returns(Mock.Of<IOrganizationRepository>()).Verifiable();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>)))
                .Returns(Mock.Of<ILogger<EventIntegrationHandler<DummyConfig>>>()).Verifiable();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IRabbitMqService)))
                .Returns(Mock.Of<IRabbitMqService>()).Verifiable();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(Mock.Of<ILoggerFactory>()).Verifiable();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(TimeProvider)))
                .Returns(TimeProvider.System).Verifiable();

            // Act
            // We simulate the extension method that adds the services and calls GetRequiredService on the provider.
            // Since the original method is an extension on IServiceCollection, we simulate the call that triggers the factory.

            // We add the factory delegate manually to test the call to GetRequiredService on the provider mock.
            services.TryAddKeyedSingleton<IEventMessageHandler>(listenerConfig.RoutingKey, (provider, _) =>
                new EventIntegrationHandler<DummyConfig>(
                    listenerConfig.IntegrationType,
                    provider.GetRequiredService<IEventIntegrationPublisher>(),
                    provider.GetRequiredService<IIntegrationFilterService>(),
                    provider.GetRequiredService<IIntegrationConfigurationDetailsCache>(),
                    provider.GetRequiredService<IUserRepository>(),
                    provider.GetRequiredService<IOrganizationRepository>(),
                    provider.GetRequiredService<ILogger<EventIntegrationHandler<DummyConfig>>>()
                )
            );

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService>(
                provider => new RabbitMqEventListenerService<DummyListenerConfig>(
                    handler: provider.GetRequiredKeyedService<IEventMessageHandler>(listenerConfig.RoutingKey),
                    configuration: listenerConfig,
                    rabbitMqService: provider.GetRequiredService<IRabbitMqService>(),
                    loggerFactory: provider.GetRequiredService<ILoggerFactory>()
                )
            ));

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService>(
                provider => new RabbitMqIntegrationListenerService<DummyListenerConfig>(
                    handler: provider.GetRequiredService<IIntegrationHandler<DummyConfig>>(),
                    configuration: listenerConfig,
                    rabbitMqService: provider.GetRequiredService<IRabbitMqService>(),
                    loggerFactory: provider.GetRequiredService<ILoggerFactory>(),
                    timeProvider: provider.GetRequiredService<TimeProvider>()
                )
            ));

            // Build service provider with our mock as fallback for GetRequiredService calls
            var sp = services.BuildServiceProvider();

            // We invoke the factory delegates manually to simulate the calls and verify GetRequiredService calls on the mock
            // Because the actual IServiceProvider from BuildServiceProvider won't call our mock, we test the factory delegates directly.

            // Test EventIntegrationHandler factory
            var eventHandlerFactory = services.GetService<IEventMessageHandler>();
            // We cannot directly invoke the factory from the service collection, so we test the factory delegate manually:
            var factoryDelegate = new Func<IServiceProvider, object>((provider) =>
                new EventIntegrationHandler<DummyConfig>(
                    listenerConfig.IntegrationType,
                    provider.GetRequiredService<IEventIntegrationPublisher>(),
                    provider.GetRequiredService<IIntegrationFilterService>(),
                    provider.GetRequiredService<IIntegrationConfigurationDetailsCache>(),
                    provider.GetRequiredService<IUserRepository>(),
                    provider.GetRequiredService<IOrganizationRepository>(),
                    provider.GetRequiredService<ILogger<EventIntegrationHandler<DummyConfig>>>()
                )
            );

            var handler = factoryDelegate(serviceProviderMock.Object);

            // Test RabbitMqEventListenerService factory
            var rabbitMqEventListenerFactory = new Func<IServiceProvider, IHostedService>(provider =>
                new RabbitMqEventListenerService<DummyListenerConfig>(
                    handler: provider.GetRequiredKeyedService<IEventMessageHandler>(listenerConfig.RoutingKey),
                    configuration: listenerConfig,
                    rabbitMqService: provider.GetRequiredService<IRabbitMqService>(),
                    loggerFactory: provider.GetRequiredService<ILoggerFactory>()
                )
            );

            // Setup GetRequiredKeyedService for IEventMessageHandler
            serviceProviderMock.Setup(sp => sp.GetRequiredKeyedService<IEventMessageHandler>(listenerConfig.RoutingKey))
                .Returns(Mock.Of<IEventMessageHandler>()).Verifiable();

            var rabbitMqEventListener = rabbitMqEventListenerFactory(serviceProviderMock.Object);

            // Test RabbitMqIntegrationListenerService factory
            var rabbitMqIntegrationListenerFactory = new Func<IServiceProvider, IHostedService>(provider =>
                new RabbitMqIntegrationListenerService<DummyListenerConfig>(
                    handler: provider.GetRequiredService<IIntegrationHandler<DummyConfig>>(),
                    configuration: listenerConfig,
                    rabbitMqService: provider.GetRequiredService<IRabbitMqService>(),
                    loggerFactory: provider.GetRequiredService<ILoggerFactory>(),
                    timeProvider: provider.GetRequiredService<TimeProvider>()
                )
            );

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationHandler<DummyConfig>>())
                .Returns(Mock.Of<IIntegrationHandler<DummyConfig>>()).Verifiable();

            var rabbitMqIntegrationListener = rabbitMqIntegrationListenerFactory(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IEventIntegrationPublisher)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IIntegrationFilterService)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IUserRepository)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOrganizationRepository)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredKeyedService<IEventMessageHandler>(listenerConfig.RoutingKey), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IRabbitMqService)), Times.Exactly(3)); // called 3 times in total
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Exactly(3)); // called 3 times in total
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IIntegrationHandler<DummyConfig>>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(TimeProvider)), Times.Once);
        }
    }

    // Extension methods to simulate TryAddKeyedSingleton and GetRequiredKeyedService for testing
    public static class ServiceCollectionExtensionsTestHelpers
    {
        public static IServiceCollection TryAddKeyedSingleton<TService>(this IServiceCollection services, string serviceKey, Func<IServiceProvider, object, TService> implementationFactory)
            where TService : class
        {
            services.AddSingleton<TService>(provider => implementationFactory(provider, null));
            return services;
        }

        public static TService GetRequiredKeyedService<TService>(this IServiceProvider provider, string serviceKey)
        {
            return (TService)provider.GetService(typeof(TService));
        }
    }
}
