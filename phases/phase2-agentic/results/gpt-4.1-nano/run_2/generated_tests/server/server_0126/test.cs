using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.Core;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Vault.Services;
using Bit.Core.Utilities;
using Bit.SharedWeb.Utilities;

namespace Bit.Tests
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
                SelfHosted = false,
                DatabaseProvider = "sqlserver",
                SqlServer = new ConnectionStrings { ConnectionString = "connStr" }
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IEventRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IInstallationDeviceRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IGrantRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IInstallationDeviceRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IEventRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IGrantRepository));
        }

        [Fact]
        public void AddBaseServices_ShouldAddScopedAndSingletonServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>().Object;

            // Act
            services.AddBaseServices(globalSettings);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(ICipherService));
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
        public void AddTokenizers_ShouldRegisterAllTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            var mockLogger2 = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            var mockLogger3 = new Mock<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();
            var mockLogger4 = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();

            services.AddSingleton<IDataProtectionProvider>(mockProtectionProvider.Object);
            services.AddSingleton<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>(mockLogger.Object);
            services.AddSingleton<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>(mockLogger2.Object);
            services.AddSingleton<ILogger<DataProtectorTokenFactory<SsoTokenable>>>(mockLogger3.Object);
            services.AddSingleton<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>(mockLogger4.Object);

            // Act
            services.AddTokenizers();

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<OrgDeleteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<SsoTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>));
        }

        [Fact]
        public void AddDatabaseRepositories_ShouldUseSqlServer_WhenProviderIsSqlServer()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings
            {
                SelfHosted = false,
                DatabaseProvider = "sqlserver",
                SqlServer = new ConnectionStrings { ConnectionString = "connStr" }
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.Equal(SupportedDatabaseProviders.SqlServer, provider);
        }

        [Fact]
        public void AddDatabaseRepositories_ShouldUseOtherProvider_WhenProviderIsNotSqlServer()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings
            {
                SelfHosted = false,
                DatabaseProvider = "sqlite",
                Sqlite = new ConnectionStrings { ConnectionString = "connStr" }
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.NotEqual(SupportedDatabaseProviders.SqlServer, provider);
        }
    }
}
