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
using Bit.Core.Services;
using Bit.Core.Settings;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
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
            var mockProvider = new Mock<IDataProtectionProvider>();
            services.AddSingleton(mockProvider.Object);
            services.AddLogging();

            // Act
            ServiceCollectionExtensions.AddTokenizers(services);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<OrgDeleteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<SsoTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>));
        }

        [Fact]
        public void AddDatabaseRepositories_ShouldConfigureCorrectProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings
            {
                SelfHosted = false
            };
            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);
            var sp = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<SupportedDatabaseProviders>(provider);
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockSettings = new Mock<IGlobalSettings>();
            mockSettings.Setup(s => s).Returns(It.IsAny<IGlobalSettings>());
            // Act
            services.AddBaseServices(mockSettings.Object);
            var sp = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(sp.GetService<ICipherService>());
            Assert.NotNull(sp.GetService<IGroupService>());
            Assert.NotNull(sp.GetService<IEventService>());
            Assert.NotNull(sp.GetService<IEmergencyAccessService>());
            Assert.NotNull(sp.GetService<IDeviceService>());
            Assert.NotNull(sp.GetService<ISsoConfigService>());
            Assert.NotNull(sp.GetService<IAuthRequestService>());
            Assert.NotNull(sp.GetService<IDuoUniversalTokenService>());
            Assert.NotNull(sp.GetService<ISendAuthorizationService>());
        }
    }
}
