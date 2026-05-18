using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Vault.Services;
using Bit.Core.HostedServices;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Platform;
using Bit.Core.Settings;

namespace Bit.SharedWeb.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDatabaseRepositories_ShouldConfigureCorrectProviderAndRepositories()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings
            {
                SelfHosted = false,
                DatabaseProvider = "sqlserver",
                SqlServer = new ConnectionStrings { ConnectionString = "Server=myServer;Database=myDb;" }
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.NotNull(provider);
            var serviceProvider = services.BuildServiceProvider();

            // Check that the correct repositories are registered
            var eventRepo = serviceProvider.GetService<IEventRepository>();
            var installRepo = serviceProvider.GetService<IInstallationDeviceRepository>();
            Assert.NotNull(eventRepo);
            Assert.NotNull(installRepo);
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            mockGlobalSettings.Setup(g => g.SelfHosted).Returns(false);
            var globalSettings = mockGlobalSettings.Object;

            // Act
            services.AddBaseServices(globalSettings);
            var provider = services.BuildServiceProvider();

            // Assert
            var cipherService = provider.GetService<ICipherService>();
            var groupService = provider.GetService<IGroupService>();
            var eventService = provider.GetService<IEventService>();
            var emergencyService = provider.GetService<IEmergencyAccessService>();
            var deviceService = provider.GetService<IDeviceService>();
            var ssoService = provider.GetService<ISsoConfigService>();
            var authRequestService = provider.GetService<IAuthRequestService>();
            var duoService = provider.GetService<IDuoUniversalTokenService>();
            var sendAuthService = provider.GetService<ISendAuthorizationService>();
            var organizationDomainService = provider.GetService<IOrganizationDomainService>();
            var vaultService = provider.GetService<IVaultService>();
            var reportingService = provider.GetService<IReportingService>();
            var keyManagementService = provider.GetService<IKeyManagementService>();
            var notificationCenterService = provider.GetService<INotificationCenterService>();
            var platformService = provider.GetService<IPlatformService>();
            var importService = provider.GetService<IImportService>();
            var sendService = provider.GetService<ISendService>();

            Assert.NotNull(cipherService);
            Assert.NotNull(groupService);
            Assert.NotNull(eventService);
            Assert.NotNull(emergencyService);
            Assert.NotNull(deviceService);
            Assert.NotNull(ssoService);
            Assert.NotNull(authRequestService);
            Assert.NotNull(duoService);
            Assert.NotNull(sendAuthService);
            Assert.NotNull(organizationDomainService);
            Assert.NotNull(vaultService);
            Assert.NotNull(reportingService);
            Assert.NotNull(keyManagementService);
            Assert.NotNull(notificationCenterService);
            Assert.NotNull(platformService);
            Assert.NotNull(importService);
            Assert.NotNull(sendService);
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterAllTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            var mockLogger2 = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            var mockLogger3 = new Mock<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();
            var mockLogger4 = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();
            var mockLogger5 = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnLog>>>();

            services.AddSingleton<IDataProtectionProvider>(mockDataProtectionProvider.Object);
            services.AddSingleton<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>(mockLogger.Object);
            services.AddSingleton<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>(mockLogger2.Object);
            services.AddSingleton<ILogger<DataProtectorTokenFactory<SsoTokenable>>>(mockLogger3.Object);
            services.AddSingleton<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>(mockLogger4.Object);
            services.AddSingleton<ILogger<DataProtectorTokenFactory<WebAuthnLog>>>(mockLogger5.Object);

            // Act
            services.AddTokenizers();
            var provider = services.BuildServiceProvider();

            // Assert
            var factory1 = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            var factory2 = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var factory3 = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var factory4 = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();
            var factory5 = provider.GetService<IDataProtectorTokenFactory<WebAuthnLog>>();

            Assert.NotNull(factory1);
            Assert.NotNull(factory2);
            Assert.NotNull(factory3);
            Assert.NotNull(factory4);
            Assert.NotNull(factory5);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterAzureServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockPublisher = new Mock<IEventIntegrationPublisher>();
            var mockFilterService = new Mock<IIntegrationFilterService>();
            var mockCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrgRepository = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockAzureServiceBus = new Mock<IAzureServiceBusService>();
            var mockHandler = new Mock<IIntegrationHandler<object>>();

            services.AddSingleton<IServiceProvider>(sp => sp);
            services.AddSingleton<IEventIntegrationPublisher>(mockPublisher.Object);
            services.AddSingleton<IIntegrationFilterService>(mockFilterService.Object);
            services.AddSingleton<IIntegrationConfigurationDetailsCache>(mockCache.Object);
            services.AddSingleton<IUserRepository>(mockUserRepository.Object);
            services.AddSingleton<IOrganizationRepository>(mockOrgRepository.Object);
            services.AddSingleton<ILogger<EventIntegrationHandler<object>>>(mockLogger.Object);
            services.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);
            services.AddSingleton<IAzureServiceBusService>(mockAzureServiceBus.Object);
            services.AddSingleton<IIntegrationHandler<object>>(mockHandler.Object);

            var listenerConfiguration = new DummyListenerConfiguration
            {
                RoutingKey = "test",
                IntegrationType = "type",
                EventPrefetchCount = 10,
                EventMaxConcurrentCalls = 5
            };

            // Act
            services.AddAzureServiceBusIntegration<object, DummyListenerConfiguration>(listenerConfiguration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var handler = serviceProvider.GetService<IEventMessageHandler>();
            Assert.NotNull(handler);
        }

        // Dummy implementation for testing
        private class DummyListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
        }
    }
}
