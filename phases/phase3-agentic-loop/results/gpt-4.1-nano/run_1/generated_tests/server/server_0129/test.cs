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
using Bit.Core.Services;
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
                SqlServer = new SqlServerSettings { ConnectionString = "Server=myServer;Database=myDb;" }
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IEventRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IInstallationDeviceRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IGrantRepository));
            Assert.IsType<SupportedDatabaseProviders>(provider);
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings();

            // Act
            services.AddBaseServices(globalSettings);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(ICipherService));
            Assert.Contains(services, s => s.ServiceType == typeof(IGroupService));
            Assert.Contains(services, s => s.ServiceType == typeof(IEventService));
            Assert.Contains(services, s => s.ServiceType == typeof(IDeviceService));
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTokenizers();

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<OrgDeleteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<SsoTokenable>));
        }

        [Fact]
        public void AddTokenizers_ShouldResolveAndUseDataProtectionProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            services.AddSingleton(mockDataProtectionProvider.Object);
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddTokenizers();

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve a token factory
            var factory = serviceProvider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            Assert.NotNull(factory);
        }
    }
}
