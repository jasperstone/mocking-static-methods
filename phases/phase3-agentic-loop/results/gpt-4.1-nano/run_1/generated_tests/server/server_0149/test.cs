using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Bit.Core;
using Bit.Core.Services;
using Bit.Core.Auth.Services;
using Bit.Core.Vault.Services;
using Bit.Core.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_Should_Register_All_TokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLogger1 = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            var mockLogger2 = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            var mockLogger3 = new Mock<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();
            var mockLogger4 = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();
            var mockLogger5 = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnLoggable>>>();
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();

            services.AddSingleton<IDataProtectionProvider>(mockDataProtectionProvider.Object);
            services.AddLogging();

            // Act
            services.AddTokenizers();

            // Assert
            var provider = services.BuildServiceProvider();

            var tokenFactory1 = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            var tokenFactory2 = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var tokenFactory3 = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var tokenFactory4 = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();
            var tokenFactory5 = provider.GetService<IDataProtectorTokenFactory<WebAuthnLoggable>>();

            Assert.NotNull(tokenFactory1);
            Assert.NotNull(tokenFactory2);
            Assert.NotNull(tokenFactory3);
            Assert.NotNull(tokenFactory4);
            Assert.NotNull(tokenFactory5);
        }
    }
}
