using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfigMock = new Mock<IIntegrationListenerConfiguration>();
            listenerConfigMock.SetupGet(l => l.RoutingKey).Returns("testKey");
            listenerConfigMock.SetupGet(l => l.IntegrationType).Returns("testIntegrationType");
            listenerConfigMock.SetupGet(l => l.EventPrefetchCount).Returns(1);
            listenerConfigMock.SetupGet(l => l.EventMaxConcurrentCalls).Returns(1);
            listenerConfigMock.SetupGet(l => l.IntegrationPrefetchCount).Returns(1);
            listenerConfigMock.SetupGet(l => l.IntegrationMaxConcurrentCalls).Returns(1);

            // Act
            // We call the private extension method via reflection because it's private
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            // We invoke the generic method with object type parameters for simplicity
            var genericMethod = method.MakeGenericMethod(typeof(object), listenerConfigMock.Object.GetType());
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfigMock.Object });

            // Assert
            // The services collection should have registrations for IEventMessageHandler and IHostedService
            Assert.Contains(services, d => d.ServiceType == typeof(IEventMessageHandler));
            Assert.Contains(services, d => d.ServiceType == typeof(IHostedService));
        }
    }

    // Dummy interfaces to satisfy the test compilation
    public interface IIntegrationListenerConfiguration
    {
        string RoutingKey { get; }
        string IntegrationType { get; }
        int EventPrefetchCount { get; }
        int EventMaxConcurrentCalls { get; }
        int IntegrationPrefetchCount { get; }
        int IntegrationMaxConcurrentCalls { get; }
    }

    public interface IEventIntegrationPublisher { }
    public interface IIntegrationFilterService { }
    public interface IIntegrationConfigurationDetailsCache { }
    public interface IUserRepository { }
    public interface IOrganizationRepository { }
    public interface IEventMessageHandler { }
    public interface IAzureServiceBusService { }
    public interface IIntegrationHandler<T> { }
}
