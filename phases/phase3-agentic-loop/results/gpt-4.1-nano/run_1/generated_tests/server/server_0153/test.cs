using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Bit.Core;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Utilities;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Logging;

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
                SelfHosted = false
            };
            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            // Check that singleton repositories are registered
            Assert.NotNull(serviceProvider.GetService<IEventRepository>());
            Assert.NotNull(serviceProvider.GetService<IInstallationDeviceRepository>());

            // Check that IGrantRepository is registered with key "cosmos"
            var grantRepo = serviceProvider.GetService<IGrantRepository>();
            Assert.NotNull(grantRepo);
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
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

            // Add DataProtection services
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
