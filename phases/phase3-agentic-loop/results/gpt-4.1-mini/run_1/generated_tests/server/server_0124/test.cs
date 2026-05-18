using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        interface IIntegrationListenerConfiguration
        {
            string RoutingKey { get; }
            string IntegrationType { get; }
            int EventPrefetchCount { get; }
            int EventMaxConcurrentCalls { get; }
        }

        class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; } = "dummyKey";
            public string IntegrationType { get; set; } = "dummyIntegration";
            public int EventPrefetchCount { get; set; } = 5;
            public int EventMaxConcurrentCalls { get; set; } = 10;
        }

        class DummyConfig { }

        [Fact]
        public void AddAzureServiceBusIntegration_InvokesGetRequiredServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // We will capture the factory delegate passed to TryAddKeyedSingleton to test invocation
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var integrationConfigurationDetailsCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<DummyConfig>>>();

            var azureServiceBusServiceMock = new Mock<IAzureServiceBusService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            var listenerConfig = new DummyListenerConfig();

            // Use reflection to get the private static method AddAzureServiceBusIntegration
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            // Act
            var genericMethod = method.MakeGenericMethod(typeof(DummyConfig), typeof(DummyListenerConfig));
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IServiceCollection>(result);

            // Find the service descriptor for IEventMessageHandler keyed by listenerConfig.RoutingKey
            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IEventMessageHandler));
            Assert.NotNull(descriptor);

            // The implementation factory is a Func<IServiceProvider, object>
            var factory = descriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Setup a mock IServiceProvider that returns the mocks above when GetRequiredService is called
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher))).Returns(eventIntegrationPublisherMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService))).Returns(integrationFilterServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(integrationConfigurationDetailsCacheMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository))).Returns(organizationRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>))).Returns(loggerMock.Object);

            // Call the factory delegate to create the handler instance
            var handler = factory(serviceProviderMock.Object);
            Assert.NotNull(handler);

            // Verify that GetService was called for each required service
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IEventIntegrationPublisher)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IIntegrationFilterService)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IUserRepository)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOrganizationRepository)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>)), Times.Once);
        }
    }

    // Dummy interfaces to satisfy the dependencies in the tested method
    public interface IEventIntegrationPublisher { }
    public interface IIntegrationFilterService { }
    public interface IIntegrationConfigurationDetailsCache { }
    public interface IUserRepository { }
    public interface IOrganizationRepository { }
    public interface IEventMessageHandler { }
    public interface IHostedService { }
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
}
