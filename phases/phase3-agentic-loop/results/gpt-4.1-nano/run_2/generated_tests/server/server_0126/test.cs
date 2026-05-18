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
using Bit.Core.Platform.Mail;
using Bit.Core.Platform.Push;
using Bit.Core.Settings;
using Microsoft.Extensions.Hosting;

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
                SqlServer = new SqlServerSettings { ConnectionString = "conn" }
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IEventRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IInstallationDeviceRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IGrantRepository));
            Assert.NotNull(provider);
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

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(ICipherService));
            Assert.Contains(services, s => s.ServiceType == typeof(IUserService));
            Assert.Contains(services, s => s.ServiceType == typeof(IOrganizationService));
            Assert.Contains(services, s => s.ServiceType == typeof(IGroupService));
            Assert.Contains(services, s => s.ServiceType == typeof(IEventService));
            Assert.Contains(services, s => s.ServiceType == typeof(IEmergencyAccessService));
            Assert.Contains(services, s => s.ServiceType == typeof(IDeviceService));
            Assert.Contains(services, s => s.ServiceType == typeof(ISsoConfigService));
            Assert.Contains(services, s => s.ServiceType == typeof(IAuthRequestService));
            Assert.Contains(services, s => s.ServiceType == typeof(IDuoUniversalTokenService));
            Assert.Contains(services, s => s.ServiceType == typeof(ISendAuthorizationService));
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            services.AddSingleton(mockProtectionProvider.Object);
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddTokenizers();

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<OrgDeleteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<SsoTokenable>));
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterServicesAndHandlers()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockHandler = new Mock<IEventMessageHandler>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();
            services.AddSingleton(mockHandler.Object);
            services.AddSingleton(mockLogger.Object);
            var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
            listenerConfig.Setup(c => c.RoutingKey).Returns("test");
            listenerConfig.Setup(c => c.IntegrationType).Returns("type");
            var service = new ServiceCollection();
            service.AddSingleton<IEventMessageHandler>(mockHandler.Object);
            var provider = service.BuildServiceProvider();

            // Act
            services.AddAzureServiceBusIntegration<object, object>(listenerConfig.Object);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IHostedService));
        }
    }
}
