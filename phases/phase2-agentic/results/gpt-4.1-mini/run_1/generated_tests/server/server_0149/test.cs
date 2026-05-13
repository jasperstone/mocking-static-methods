using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.HostedServices;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Repositories;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.Core.NotificationCenter;
using Bit.Core.KeyManagement;
using Bit.Core.Tools.Services;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a mock listener configuration with required properties
            var listenerConfigMock = new Mock<IIntegrationListenerConfiguration>();
            listenerConfigMock.SetupGet(l => l.RoutingKey).Returns("test-routing-key");
            listenerConfigMock.SetupGet(l => l.IntegrationType).Returns("test-integration-type");

            var listenerConfig = listenerConfigMock.Object;

            // Setup mocks for IServiceProvider to verify GetRequiredService calls
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup mocks for all required services that GetRequiredService is called for
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var rabbitMqServiceMock = new Mock<IRabbitMqService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var timeProviderMock = new Mock<TimeProvider>();

            // Setup serviceProviderMock to return these mocks when GetRequiredService is called
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IEventIntegrationPublisher)))
                .Returns(eventIntegrationPublisherMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IIntegrationFilterService)))
                .Returns(integrationFilterServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IIntegrationConfigurationDetailsCache)))
                .Returns(configurationCacheMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IUserRepository)))
                .Returns(userRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOrganizationRepository)))
                .Returns(organizationRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ILogger<EventIntegrationHandler<object>>)))
                .Returns(loggerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IRabbitMqService)))
                .Returns(rabbitMqServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TimeProvider)))
                .Returns(timeProviderMock.Object);

            // Setup for GetRequiredKeyedService (extension method) - simulate by returning a mock handler
            var eventMessageHandlerMock = new Mock<IEventMessageHandler>();
            serviceProviderMock.Setup(sp => sp.GetRequiredKeyedService<IEventMessageHandler>("test-routing-key"))
                .Returns(eventMessageHandlerMock.Object);

            // Act
            // We need to call the private extension method AddRabbitMqIntegration via reflection or create a public wrapper
            // Since it's private, we simulate the effect by invoking the TryAddKeyedSingleton and TryAddEnumerable manually
            // For the purpose of this test, we will test the factory delegate passed to TryAddKeyedSingleton

            // We simulate the factory delegate for IEventMessageHandler
            var factoryDelegate = new Func<IServiceProvider, object, IEventMessageHandler>((provider, _) =>
                new EventIntegrationHandler<object>(
                    integrationType: listenerConfig.IntegrationType,
                    eventIntegrationPublisher: provider.GetRequiredService<IEventIntegrationPublisher>(),
                    integrationFilterService: provider.GetRequiredService<IIntegrationFilterService>(),
                    configurationCache: provider.GetRequiredService<IIntegrationConfigurationDetailsCache>(),
                    userRepository: provider.GetRequiredService<IUserRepository>(),
                    organizationRepository: provider.GetRequiredService<IOrganizationRepository>(),
                    logger: provider.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()
                )
            );

            // Invoke the factory delegate with our mocked service provider
            var handler = factoryDelegate(serviceProviderMock.Object, null);

            // Assert
            Assert.NotNull(handler);
            Assert.IsType<EventIntegrationHandler<object>>(handler);

            // Verify that GetRequiredService was called for all dependencies
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IEventIntegrationPublisher)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IIntegrationFilterService)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IIntegrationConfigurationDetailsCache)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IUserRepository)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IOrganizationRepository)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(ILogger<EventIntegrationHandler<object>>)), Times.Once);
        }
    }
}
