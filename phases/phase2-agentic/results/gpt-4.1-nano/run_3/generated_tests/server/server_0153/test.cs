using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;

namespace Bit.Tests.Utilities
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
            var mockDataProtection = new Mock<IDataProtectionProvider>();
            services.AddSingleton(mockDataProtectionProvider.Object);
            services.AddSingleton(mockLogger.Object);
            services.AddSingleton(mockLogger2.Object);
            services.AddSingleton(mockLogger3.Object);
            services.AddSingleton(mockLogger4.Object);
            services.AddSingleton(mockDataProtection.Object);

            // Act
            services.AddTokenizers();

            // Assert
            var provider = services.BuildServiceProvider();
            var tokenFactory1 = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            var tokenFactory2 = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var tokenFactory3 = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var tokenFactory4 = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();

            Assert.NotNull(tokenFactory1);
            Assert.NotNull(tokenFactory2);
            Assert.NotNull(tokenFactory3);
            Assert.NotNull(tokenFactory4);
        }

        [Fact]
        public void AddDatabaseRepositories_ShouldConfigureCorrectProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>();
            var providerType = SupportedDatabaseProviders.SqlServer;
            var connectionString = "Server=myServer;Database=myDb;";

            globalSettings.Setup(g => g.SelfHosted).Returns(false);
            globalSettings.Setup(g => g.GetDatabaseProvider()).Returns((providerType, connectionString));

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings.Object);

            // Assert
            Assert.IsType<SupportedDatabaseProviders>(provider);
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            mockGlobalSettings.Setup(g => g).Returns(() => null);

            // Act
            services.AddBaseServices(mockGlobalSettings.Object);

            // Assert
            var provider = services.BuildServiceProvider();
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
        public void AddTokenizers_ShouldRegisterMultipleTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            services.AddSingleton(mockDataProtectionProvider.Object);
            services.AddLogging();

            // Act
            services.AddTokenizers();

            // Assert
            var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>());
        }
    }
}
