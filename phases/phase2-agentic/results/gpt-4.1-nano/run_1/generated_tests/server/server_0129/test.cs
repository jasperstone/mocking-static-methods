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
using Bit.Core.Settings;

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
                // Set other necessary properties if needed
            };
            // Mock GetDatabaseProvider to return a specific provider and connection string
            var providerType = SupportedDatabaseProviders.SqlServer;
            var connectionString = "Server=myServer;Database=myDb;User Id=myUser;Password=myPass;";
            // Use a delegate or extension method override if possible, or test indirectly

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.NotNull(provider);
            Assert.Contains(typeof(IEventRepository), services);
            Assert.Contains(typeof(IInstallationDeviceRepository), services);
            // Additional assertions can be added based on the setup
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
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

        // Additional tests can be added for other extension methods as needed
    }
}
