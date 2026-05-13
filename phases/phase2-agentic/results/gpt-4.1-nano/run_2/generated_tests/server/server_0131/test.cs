using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using Bit.Core.HostedServices;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDatabaseRepositories_ShouldConfigureServices_BasedOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings
            {
                SelfHosted = false
            };
            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IEventRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IInstallationDeviceRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IGrantRepository));
        }

        [Fact]
        public void AddBaseServices_ShouldAddCoreServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>().Object;

            // Act
            services.AddBaseServices(globalSettings);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<ICipherService>());
            Assert.NotNull(provider.GetService<IGroupService>());
            Assert.NotNull(provider.GetService<IEventService>());
            Assert.NotNull(provider.GetService<IEmergencyAccessService>());
            Assert.NotNull(provider.GetService<IDeviceService>());
            Assert.NotNull(provider.GetService<ISsoConfigService>());
            Assert.NotNull(provider.GetService<IAuthRequestService>());
            Assert.NotNull(provider.GetService<IDuoUniversalTokenService>());
            Assert.NotNull(provider.GetService<ISendAuthorizationService>());
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDataProtection();

            // Act
            services.AddTokenizers();
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>());
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ShouldAddHostedServicesAndHandlers()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockHandler = new Mock<IEventMessageHandler>();
            var mockHandler2 = new Mock<IIntegrationHandler<SomeConfig>>();
            var listenerConfig = new SomeConfig
            {
                RoutingKey = "test",
                IntegrationType = "type",
                EventPrefetchCount = 10,
                EventMaxConcurrentCalls = 5,
                IntegrationPrefetchCount = 20,
                IntegrationMaxConcurrentCalls = 10
            };

            // Setup provider to return required services
            mockProvider.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(Mock.Of<IEventIntegrationPublisher>());
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationFilterService>())
                .Returns(Mock.Of<IIntegrationFilterService>());
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            mockProvider.Setup(p => p.GetRequiredService<IUserRepository>())
                .Returns(Mock.Of<IUserRepository>());
            mockProvider.Setup(p => p.GetRequiredService<IOrganizationRepository>())
                .Returns(Mock.Of<IOrganizationRepository>());
            mockProvider.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<SomeConfig>>>())
                .Returns(Mock.Of<ILogger<EventIntegrationHandler<SomeConfig>>>());
            mockProvider.Setup(p => p.GetRequiredService<IAzureServiceBusService>())
                .Returns(Mock.Of<IAzureServiceBusService>());
            mockProvider.Setup(p => p.GetRequiredService<ILoggerFactory>())
                .Returns(mockLoggerFactory.Object);
            mockProvider.Setup(p => p.GetRequiredKeyedService<IEventMessageHandler>(It.IsAny<string>()))
                .Returns(Mock.Of<IEventMessageHandler>());

            // Act
            services.AddAzureServiceBusIntegration<SomeConfig, SomeConfig>(listenerConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var hostedServices = serviceProvider.GetServices<IHostedService>();
            Assert.Contains(hostedServices, s => s.GetType() == typeof(AzureServiceBusEventListenerService<SomeConfig>));
            Assert.Contains(hostedServices, s => s.GetType() == typeof(AzureServiceBusIntegrationListenerService<SomeConfig>));
        }
    }

    // Dummy config class for testing
    public class SomeConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey { get; set; }
        public string IntegrationType { get; set; }
        public int EventPrefetchCount { get; set; }
        public int EventMaxConcurrentCalls { get; set; }
        public int IntegrationPrefetchCount { get; set; }
        public int IntegrationMaxConcurrentCalls { get; set; }
    }
}
