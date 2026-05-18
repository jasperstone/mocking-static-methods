using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Bit.Core;
using Bit.Core.Services;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventRepo = serviceProvider.GetService<IEventRepository>();
            var installRepo = serviceProvider.GetService<IInstallationDeviceRepository>();
            var grantRepo = serviceProvider.GetService<IGrantRepository>();

            Assert.NotNull(eventRepo);
            Assert.NotNull(installRepo);
            Assert.NotNull(grantRepo);
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
            var cipherService = provider.GetService<ICipherService>();
            var userService = provider.GetService<IUserService>();
            var reportService = provider.GetService<IReportingService>();
            var keyManagementService = provider.GetService<IKeyManagementService>();
            var notificationCenter = provider.GetService<INotificationCenterService>();
            var platformService = provider.GetService<IPlatformService>();
            var importService = provider.GetService<IImportService>();
            var sendService = provider.GetService<ISendService>();

            Assert.NotNull(cipherService);
            Assert.NotNull(userService);
            Assert.NotNull(reportService);
            Assert.NotNull(keyManagementService);
            Assert.NotNull(notificationCenter);
            Assert.NotNull(platformService);
            Assert.NotNull(importService);
            Assert.NotNull(sendService);
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTokenizers();

            // Assert
            var provider = services.BuildServiceProvider();
            var factory = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterMultipleTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTokenizers();

            // Assert
            var provider = services.BuildServiceProvider();
            var ssoFactory = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var webAuthnFactory = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();
            Assert.NotNull(ssoFactory);
            Assert.NotNull(webAuthnFactory);
        }
    }

    // Dummy classes to satisfy the code dependencies
    public class GlobalSettings : IGlobalSettings
    {
        public bool SelfHosted { get; set; }
    }

    public interface IGlobalSettings
    {
        bool SelfHosted { get; }
    }

    public class OrgDeleteTokenable
    {
        public const string ClearTextPrefix = "org";
        public const string DataProtectorPurpose = "OrgPurpose";
    }

    public class SsoTokenable
    {
        public const string ClearTextPrefix = "sso";
        public const string DataProtectorPurpose = "SsoPurpose";
    }

    public class WebAuthnCredentialCreateOptionsTokenable
    {
        public const string ClearTextPrefix = "webauthn";
        public const string DataProtectorPurpose = "WebAuthnPurpose";
    }
}
