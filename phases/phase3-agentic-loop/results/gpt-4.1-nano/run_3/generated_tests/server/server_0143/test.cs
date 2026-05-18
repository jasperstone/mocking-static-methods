using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Bit.Core;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Auth.Services;
using Bit.Core.Vault.Services;
using Microsoft.Extensions.Logging;

namespace Bit.SharedWeb.Utilities.Tests
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
            Assert.NotNull(serviceProvider.GetService<IEventRepository>());
            Assert.NotNull(serviceProvider.GetService<IInstallationDeviceRepository>());
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockSettings = new Mock<IGlobalSettings>();
            // Act
            services.AddBaseServices(mockSettings.Object);
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
    }
}
