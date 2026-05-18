using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.HostedServices;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Repositories;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_CallsGetRequiredServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockProvider = new Mock<IServiceProvider>();

            // Setup GetRequiredService calls to return mocks for expected types
            mockProvider.Setup(p => p.GetService(typeof(IEventIntegrationPublisher))).Returns(Mock.Of<IEventIntegrationPublisher>());
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationFilterService))).Returns(Mock.Of<IIntegrationFilterService>());
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            mockProvider.Setup(p => p.GetService(typeof(IUserRepository))).Returns(Mock.Of<IUserRepository>());
            mockProvider.Setup(p => p.GetService(typeof(IOrganizationRepository))).Returns(Mock.Of<IOrganizationRepository>());
            mockProvider.Setup(p => p.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>))).Returns(Mock.Of<ILogger<EventIntegrationHandler<DummyConfig>>>());
            mockProvider.Setup(p => p.GetService(typeof(IRabbitMqService))).Returns(Mock.Of<IRabbitMqService>());
            mockProvider.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());
            mockProvider.Setup(p => p.GetService(typeof(TimeProvider))).Returns(Mock.Of<TimeProvider>());
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationHandler<DummyConfig>))).Returns(Mock.Of<IIntegrationHandler<DummyConfig>>());
            mockProvider.Setup(p => p.GetService(typeof(IEventMessageHandler))).Returns(Mock.Of<IEventMessageHandler>());

            var listenerConfig = new DummyListenerConfiguration();

            // Use reflection to get the private static method AddRabbitMqIntegration
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddRabbitMqIntegration", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            // Act
            // The method adds services with factories that will call GetRequiredService on IServiceProvider
            // We invoke the method to add those services
            var result = method.Invoke(null, new object[] { services, listenerConfig });
            Assert.Same(services, result);

            // Now build the service provider from the collection, but override IServiceProvider to our mock
            // We will invoke the factories manually to verify they call GetRequiredService on our mockProvider

            foreach (var serviceDescriptor in services)
            {
                if (serviceDescriptor.ImplementationFactory != null)
                {
                    // Call the factory with our mockProvider
                    serviceDescriptor.ImplementationFactory(mockProvider.Object);
                }
            }

            // Assert
            // Verify that GetService was called for the expected service types at least once
            mockProvider.Verify(p => p.GetService(typeof(IEventIntegrationPublisher)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IIntegrationFilterService)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IUserRepository)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IOrganizationRepository)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IRabbitMqService)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(TimeProvider)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IIntegrationHandler<DummyConfig>)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IEventMessageHandler)), Times.AtLeastOnce);
        }

        private class DummyListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey => "dummy-routing-key";
            public string IntegrationType => "dummy-integration-type";
        }

        private class DummyConfig { }
    }
}
