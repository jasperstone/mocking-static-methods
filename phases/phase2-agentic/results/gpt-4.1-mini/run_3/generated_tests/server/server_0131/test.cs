using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        // We will test the AddAzureServiceBusIntegration method indirectly by creating a minimal IServiceCollection,
        // adding the required services, and verifying that the services are registered and that the factory calls GetRequiredService.

        // Since the method is private, we will use reflection to invoke it.

        // We will create mocks for the required services and verify that the factory method calls GetRequiredService on the provider.

        // The method signature:
        // private static IServiceCollection AddAzureServiceBusIntegration<TConfig, TListenerConfig>(this IServiceCollection services,
        //     TListenerConfig listenerConfiguration)
        //     where TConfig : class
        //     where TListenerConfig : IIntegrationListenerConfiguration

        // We need to define minimal interfaces/classes to satisfy the generic constraints and parameters.

        interface IIntegrationListenerConfiguration
        {
            string RoutingKey { get; }
            string IntegrationType { get; }
            int EventPrefetchCount { get; }
            int EventMaxConcurrentCalls { get; }
            int IntegrationPrefetchCount { get; }
            int IntegrationMaxConcurrentCalls { get; }
        }

        class TestListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
            public int IntegrationPrefetchCount { get; set; }
            public int IntegrationMaxConcurrentCalls { get; set; }
        }

        class TestConfig { }

        // We need to mock the following interfaces used in the factory:
        // IEventIntegrationPublisher, IIntegrationFilterService, IIntegrationConfigurationDetailsCache,
        // IUserRepository, IOrganizationRepository, ILogger<EventIntegrationHandler<TConfig>>,
        // IEventMessageHandler, IAzureServiceBusService, ILoggerFactory, IIntegrationHandler<TConfig>

        // We will create mocks and register them in the service collection.

        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new TestListenerConfig
            {
                RoutingKey = "test-key",
                IntegrationType = "test-integration",
                EventPrefetchCount = 5,
                EventMaxConcurrentCalls = 10,
                IntegrationPrefetchCount = 7,
                IntegrationMaxConcurrentCalls = 14
            };

            // Create mocks for all required services
            var eventIntegrationPublisherMock = new Mock<object>();
            var integrationFilterServiceMock = new Mock<object>();
            var integrationConfigurationDetailsCacheMock = new Mock<object>();
            var userRepositoryMock = new Mock<object>();
            var organizationRepositoryMock = new Mock<object>();
            var loggerMock = new Mock<ILogger>();

            var eventMessageHandlerMock = new Mock<object>();
            var azureServiceBusServiceMock = new Mock<object>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var integrationHandlerMock = new Mock<object>();

            // We need to register these mocks with the correct service types
            // The actual types are interfaces, so we will use the interface types from the original code
            // But since we don't have the actual interface types here, we will use the same mocks with object type
            // and register them as the required service types using the Type objects from reflection.

            // To get the types, we will use reflection on the ServiceCollectionExtensions class to get the method and parameter types.

            // But since we don't have the actual types here, we will register the mocks as the service types by their interface names.

            // For the purpose of this test, we will register the mocks as the service types by their interface names using Type.GetType.

            // We will create a helper method to register mocks by interface name.

            void RegisterMock<T>(IServiceCollection sc, Mock<T> mock) where T : class
            {
                sc.AddSingleton(typeof(T), mock.Object);
            }

            // We will define minimal interfaces to register mocks for the required services.

            // Define minimal interfaces to register mocks
            services.AddSingleton(Mock.Of<IEventIntegrationPublisher>());
            services.AddSingleton(Mock.Of<IIntegrationFilterService>());
            services.AddSingleton(Mock.Of<IIntegrationConfigurationDetailsCache>());
            services.AddSingleton(Mock.Of<IUserRepository>());
            services.AddSingleton(Mock.Of<IOrganizationRepository>());
            services.AddSingleton(Mock.Of<ILogger<EventIntegrationHandler<TestConfig>>>());
            services.AddSingleton(Mock.Of<IAzureServiceBusService>());
            services.AddSingleton(Mock.Of<ILoggerFactory>());
            services.AddSingleton(Mock.Of<IIntegrationHandler<TestConfig>>());

            // We also need to add the extension methods TryAddKeyedSingleton and TryAddEnumerable
            // Since these are extension methods, we assume they are available in the project.
            // We will just call the private method via reflection.

            // Act
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);

            var genericMethod = method.MakeGenericMethod(typeof(TestConfig), typeof(TestListenerConfig));

            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IServiceCollection>(result);

            var serviceCollection = (IServiceCollection)result;

            // Verify that the services collection contains the expected service descriptors for IHostedService
            var hostedServices = serviceCollection.Where(sd => sd.ServiceType == typeof(IHostedService)).ToList();
            Assert.NotEmpty(hostedServices);

            // Verify that the service descriptors have factories that call GetRequiredService on the provider
            // We can create a mock IServiceProvider and verify that GetRequiredService is called when the factory is invoked

            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetRequiredService to return mocks for the requested types
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher))).Returns(Mock.Of<IEventIntegrationPublisher>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService))).Returns(Mock.Of<IIntegrationFilterService>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(Mock.Of<IUserRepository>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository))).Returns(Mock.Of<IOrganizationRepository>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<TestConfig>>))).Returns(Mock.Of<ILogger<EventIntegrationHandler<TestConfig>>>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAzureServiceBusService))).Returns(Mock.Of<IAzureServiceBusService>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationHandler<TestConfig>))).Returns(Mock.Of<IIntegrationHandler<TestConfig>>());

            // We will invoke the factory of the first TryAddKeyedSingleton service descriptor to verify GetRequiredService calls

            var keyedSingletonDescriptor = serviceCollection.FirstOrDefault(sd => sd.ImplementationFactory != null && sd.ServiceType == typeof(IEventMessageHandler));
            Assert.NotNull(keyedSingletonDescriptor);

            // The factory has signature Func<IServiceProvider, object>
            var factory = keyedSingletonDescriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Invoke the factory with the mock service provider
            var instance = factory(serviceProviderMock.Object);

            Assert.NotNull(instance);

            // Verify that GetService was called for the required services
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IEventIntegrationPublisher)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IIntegrationFilterService)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IUserRepository)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOrganizationRepository)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<TestConfig>>)), Times.AtLeastOnce);
        }
    }

    // Minimal interface definitions to satisfy the compiler for the test
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
            object serviceBusOptions,
            ILoggerFactory loggerFactory)
        {
        }
        public System.Threading.Tasks.Task StartAsync(System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task StopAsync(System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
    }
    public class AzureServiceBusIntegrationListenerService<TListenerConfig> : IHostedService
    {
        public AzureServiceBusIntegrationListenerService(TListenerConfig configuration,
            IIntegrationHandler<TestConfig> handler,
            IAzureServiceBusService serviceBusService,
            object serviceBusOptions,
            ILoggerFactory loggerFactory)
        {
        }
        public System.Threading.Tasks.Task StartAsync(System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task StopAsync(System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
    }
}
