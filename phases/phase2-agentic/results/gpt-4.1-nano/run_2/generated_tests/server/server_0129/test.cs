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
using Microsoft.AspNetCore.DataProtection;

namespace Bit.SharedWeb.Tests.Utilities
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
                SqlServer = new SqlServerSettings { ConnectionString = "conn" }
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.NotNull(provider);
            var serviceProvider = services.BuildServiceProvider();

            // Check that singleton repositories are registered
            var eventRepo = serviceProvider.GetService<IEventRepository>();
            var installRepo = serviceProvider.GetService<IInstallationDeviceRepository>();
            var grantRepo = serviceProvider.GetService<IGrantRepository>();

            Assert.NotNull(eventRepo);
            Assert.NotNull(installRepo);
            Assert.NotNull(grantRepo);
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
            services.AddLogging();

            // Act
            services.AddTokenizers();

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            var factory1 = serviceProvider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            var factory2 = serviceProvider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var factory3 = serviceProvider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var factory4 = serviceProvider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();

            Assert.NotNull(factory1);
            Assert.NotNull(factory2);
            Assert.NotNull(factory3);
            Assert.NotNull(factory4);
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProtectionProvider = new Mock<IDataProtectionProvider>();
            services.AddSingleton<IDataProtectionProvider>(mockProtectionProvider.Object);
            services.AddLogging();

            // Act
            services.AddBaseServices(new GlobalSettings());

            // Assert
            var provider = services.BuildServiceProvider();

            var cipherService = provider.GetService<ICipherService>();
            var userService = provider.GetService<IUserService>();
            var reportService = provider.GetService<IReportingService>();
            var notificationService = provider.GetService<INotificationCenterService>();

            Assert.NotNull(cipherService);
            Assert.NotNull(userService);
            Assert.NotNull(reportService);
            Assert.NotNull(notificationService);
        }

        [Fact]
        public void AddTokenizers_ShouldUseGetDataProtectionProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProtectionProvider = new Mock<IDataProtectionProvider>();
            services.AddSingleton<IDataProtectionProvider>(mockProtectionProvider.Object);
            services.AddLogging();

            // Act
            services.AddTokenizers();

            // Assert
            var provider = services.BuildServiceProvider();

            var factory = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            Assert.NotNull(factory);
        }
    }
}
